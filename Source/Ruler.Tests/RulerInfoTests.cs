using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ruler.Shared.Factories;
using Ruler.Shared.Interfaces;
using Ruler.Shared.Models;

namespace Ruler.Test
{
	[TestClass]
	public class RulerInfoTests
	{
		[TestMethod]
		public void GetDefaultRulerInfoTest()
		{
			Assert.AreNotEqual(null, RulerFactory.CreateDefault());
		}

		[TestMethod]
		public void CopyIntoTest()
		{
			RulerInfo source = RulerFactory.CreateDefault();
			source.Width = 500;
			source.Height = 80;
			source.Opacity = 0.90;
			source.ShowToolTip = true;
			source.IsLocked = true;
			source.IsVertical = true;
			source.TopMost = true;
          
			RulerInfo target = RulerFactory.CreateDefault();
			target.Width = 200;
			target.Height = 50;
			target.Opacity = 0.50;
			target.ShowToolTip = false;
			target.IsLocked = false;
			target.IsVertical = false;
			target.TopMost = false;


			RulerFactory.CopyValues(source, target);

			var properties = Helper.GetPublicPropertiesFromInterface(typeof(IRulerInfo));

			foreach (PropertyInfo pi in properties)
			{
				Assert.AreEqual(pi.GetValue(source), pi.GetValue(target));
			}
		}
	}
}