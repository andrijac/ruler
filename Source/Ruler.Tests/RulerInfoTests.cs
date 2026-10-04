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
		private readonly IRulerFactory _rulerFactory;

		public RulerInfoTests(IRulerFactory rulerFactory)
		{
			_rulerFactory = rulerFactory;
		}

		[TestMethod]
		public void GetDefaultRulerInfoTest()
		{
			Assert.AreNotEqual(null, _rulerFactory.CreateDefault());
		}

		[TestMethod]
		public void CopyIntoTest()
		{
			RulerInfo source = _rulerFactory.CreateDefault();
			source.Width = 500;
			source.Height = 80;
			source.Opacity = 0.90;
			source.ShowToolTip = true;
			source.IsLocked = true;
			source.IsVertical = true;
			source.TopMost = true;
          
			RulerInfo target = _rulerFactory.CreateDefault();
			target.Width = 200;
			target.Height = 50;
			target.Opacity = 0.50;
			target.ShowToolTip = false;
			target.IsLocked = false;
			target.IsVertical = false;
			target.TopMost = false;


			_rulerFactory.CopyValues(source, target);

			var properties = Helper.GetPublicPropertiesFromInterface(typeof(IRulerInfo));

			foreach (PropertyInfo pi in properties)
			{
				Assert.AreEqual(pi.GetValue(source), pi.GetValue(target));
			}
		}
	}
}