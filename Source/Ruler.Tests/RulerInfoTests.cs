using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ruler.Test
{
	[TestClass]
	public class RulerInfoTests
	{
		[TestMethod]
		public void GetDefaultRulerInfoTest()
		{
			Assert.AreNotEqual(null, RulerInfo.GetDefaultRulerInfo());
		}

		[TestMethod]
		public void CopyIntoTest()
		{
			IRulerInfo source = new RulerInfo
			{
				Width = 500,
				Height = 80,
				Opacity = 0.90,
				ShowToolTip = true,
				IsLocked = true,
				IsVertical = true,
				TopMost = true
			};

			IRulerInfo target = new RulerInfo
			{
				Width = 400,
				Height = 75,
				Opacity = 0.60,
				ShowToolTip = true,
				IsLocked = false,
				IsVertical = false,
				TopMost = true
			};

			RulerInfo.CopyInto(source, target);

			var properties = Helper.GetPublicPropertiesFromInterface(typeof(IRulerInfo));

			foreach (PropertyInfo pi in properties)
			{
				Assert.AreEqual(pi.GetValue(source), pi.GetValue(target));
			}
		}
	}
}