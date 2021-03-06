using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.PropertyEditing.Drawing;

namespace Xamarin.PropertyEditing.Tests
{
	public class MockResourceProvider
		: IResourceProvider
	{
		public bool CanCreateResources => true;

		public Task<ResourceCreateError> CheckNameErrorsAsync (object target, ResourceSource source, string name)
		{
			ResourceCreateError error = null;
			if (this.resources[source].Any (r => r.Name == name)) {
				error = new ResourceCreateError ("Name in use", isWarning: false);
			} else {
				var order = new List<ResourceSourceType> {
					ResourceSourceType.Document,
					ResourceSourceType.ResourceDictionary,
					ResourceSourceType.Application,
					ResourceSourceType.System,
				};

				// Simplistic example of hierarchy override checking
				for (int i = order.IndexOf (source.Type)+1; i < order.Count; i++) {
					if (this.resources.Where (ig => ig.Key.Type == order[i]).SelectMany (ig => ig).Any (r => r.Name == name)) {
						error = new ResourceCreateError ("Resource would override another resource", isWarning: true);
						break;
					}
				}
			}

			return Task.FromResult (error);
		}

		public Task<Resource> CreateResourceAsync<T> (ResourceSource source, string name, T value)
		{
			var r = new Resource<T> (source, name, value);
			((ObservableLookup<ResourceSource, Resource>)this.resources).Add (source, r);
			return Task.FromResult<Resource> (r);
		}

		public Task<IReadOnlyList<Resource>> GetResourcesAsync (object target, CancellationToken cancelToken)
		{
			return Task.FromResult<IReadOnlyList<Resource>> (this.resources.SelectMany (g => g)
				.Where (r => !(r.Source is ObjectResourceSource ors) || ReferenceEquals (target, ors.Target))
				.ToList ());
		}

		public Task<IReadOnlyList<Resource>> GetResourcesAsync (object target, IPropertyInfo property, CancellationToken cancelToken)
		{
			return Task.FromResult<IReadOnlyList<Resource>> (this.resources.SelectMany (g => g)
				.Where (r => property.Type.IsAssignableFrom (r.GetType().GetGenericArguments()[0]) && (!(r.Source is ObjectResourceSource ors) || ReferenceEquals (target, ors.Target)))
				.ToList());
		}

		Task<IReadOnlyList<ResourceSource>> IResourceProvider.GetResourceSourcesAsync (object target)
		{
			return MockResourceProvider.GetResourceSourcesAsync (target);
		}

		public Task<IReadOnlyList<ResourceSource>> GetResourceSourcesAsync (object target, IPropertyInfo property)
		{
			return GetResourceSourcesAsync (target);
		}

		public static Task<IReadOnlyList<ResourceSource>> GetResourceSourcesAsync (object target)
		{
			return Task.FromResult<IReadOnlyList<ResourceSource>> (new[] { SystemResourcesSource, ApplicationResourcesSource, Resources, Window, new ObjectResourceSource (target, target.GetType ().Name, ResourceSourceType.Document) });
		}

		public Task<string> SuggestResourceNameAsync (IReadOnlyCollection<object> targets, IPropertyInfo property)
		{
			return SuggestResourceNameAsync (targets, property.RealType);
		}

		public Task<string> SuggestResourceNameAsync (IReadOnlyCollection<object> targets, ITypeInfo resourceType)
		{
			int i = 1;
			string key;
			do {
				key = resourceType.Name + i++;
			} while (this.resources[ApplicationResourcesSource].Any (r => r.Name == key));

			return Task.FromResult (key);
		}

		private class ObjectResourceSource
			: ResourceSource
		{
			public ObjectResourceSource (object target, string name, ResourceSourceType type)
				: base (name, type)
			{
				if (target == null)
					throw new ArgumentNullException (nameof(target));

				this.target = target;
			}

			public object Target => this.target;

			public override int GetHashCode ()
			{
				int hashCode = base.GetHashCode ();
				unchecked {
					hashCode = (hashCode * 397) ^ this.target.GetHashCode();
				}

				return hashCode;
			}

			public override bool Equals (ResourceSource other)
			{
				if (!base.Equals (other))
					return false;

				return (other is ObjectResourceSource ors && ReferenceEquals (ors.target, this.target));
			}

			private readonly object target;
		}

		internal static readonly ResourceSource SystemResourcesSource = new ResourceSource ("System Resources", ResourceSourceType.System);
		internal static readonly ResourceSource ApplicationResourcesSource = new ResourceSource ("App resources", ResourceSourceType.Application);
		private static readonly ResourceSource Resources = new ResourceSource ("Resources.xaml", ResourceSourceType.ResourceDictionary);
		private static readonly ResourceSource Window = new ResourceSource ("Window: <no name>", ResourceSourceType.Document);

		private readonly ILookup<ResourceSource, Resource> resources = new ObservableLookup<ResourceSource, Resource> {
			new ObservableGrouping<ResourceSource, Resource> (SystemResourcesSource) {
				new Resource<CommonSolidBrush> (SystemResourcesSource, "ControlTextBrush", new CommonSolidBrush (0, 0, 0)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "HighlightBrush", new CommonSolidBrush (51, 153, 255)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "TransparentBrush", new CommonSolidBrush (0, 0, 0, 0)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "ATextBrush", new CommonSolidBrush (0, 0, 0)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "ATransparentBrush", new CommonSolidBrush (51, 153, 255)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "AHighlightBrush", new CommonSolidBrush (0, 0, 0, 0)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "BTextBrush", new CommonSolidBrush (0, 0, 0)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "BHighlightBrush", new CommonSolidBrush (51, 153, 255)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "BTransparentBrush", new CommonSolidBrush (0, 0, 0, 0)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "CTextBrush", new CommonSolidBrush (0, 0, 0)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "CHighlightBrush", new CommonSolidBrush (51, 153, 255)),
				new Resource<CommonSolidBrush> (SystemResourcesSource, "CTransparentBrush", new CommonSolidBrush (0, 0, 0, 0)),
				new Resource<CommonColor> (SystemResourcesSource, "ControlTextColor", new CommonColor (0, 0, 0)),
				new Resource<CommonColor> (SystemResourcesSource, "HighlightColor", new CommonColor (51, 153, 255))
			},

			new ObservableGrouping<ResourceSource, Resource> (ApplicationResourcesSource) {
				new Resource<CommonSolidBrush> (SystemResourcesSource, "CustomHighlightBrush", new CommonSolidBrush (255, 165, 0)),
			}
		};
	}
}
