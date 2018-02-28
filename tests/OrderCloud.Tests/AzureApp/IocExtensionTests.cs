using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace OrderCloud.AzureApp.Tests
{
	[TestFixture]
	public class IocExtensionTests
	{
		[Test]
		public void can_register_services_by_convention_without_namespace() {
			var container = Substitute.For<IServiceCollection>();

			container.AddByConvention(this.GetType().Assembly);

			container.Received(3).Add(Arg.Any<ServiceDescriptor>());
			container.Received(1).Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(MyServices.ISrv1) && d.ImplementationType == typeof(MyServices.Srv1)));
			container.Received(1).Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(MyServices.ISrv2) && d.ImplementationType == typeof(MyServices.Srv2)));
			container.Received(1).Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(MyOtherServices.ISrv4) && d.ImplementationType == typeof(MyOtherServices.Srv4)));
		}

		[Test]
		public void can_register_services_by_convention_with_namespace() {
			var container = Substitute.For<IServiceCollection>();

			container.AddByConvention(this.GetType().Assembly, "OrderCloud.AzureApp.Tests.MyServices");

			container.Received(2).Add(Arg.Any<ServiceDescriptor>());
			container.Received(1).Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(MyServices.ISrv1) && d.ImplementationType == typeof(MyServices.Srv1)));
			container.Received(1).Add(Arg.Is<ServiceDescriptor>(d => d.ServiceType == typeof(MyServices.ISrv2) && d.ImplementationType == typeof(MyServices.Srv2)));
		}
	}

	namespace MyServices
	{
		public interface ISrv1 { }
		public interface ISrv2 { }
		public interface Srv1 : ISrv1 { }
		public interface Srv2 : ISrv1, ISrv2 { }
		public interface Srv3 : ISrv1 { }
	}

	namespace MyOtherServices
	{
		public interface ISrv4 { }
		public interface Srv4 : ISrv4 { }
	}
}
