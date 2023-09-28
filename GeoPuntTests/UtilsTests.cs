
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.CIM;
using GeoPunt;
using GeoPunt.Helpers;
using System.Net;

namespace GeopuntTests
{
    [TestClass]
    public class UtilsTests
    {

        Utils utils = new Utils();

        [TestMethod]
        public void TestExportToGeoJson()
        {

            MapPoint mapPoint = MapPointBuilderEx.CreateMapPoint(4.20, 5.20);
            List<Graphic> graphics = new List<Graphic>();
            graphics.Add(new Graphic(new Dictionary<string, object>
                {
                                    {"mon address", "Chez moi"},
                                }, mapPoint));
            // utils.ExportToGeoJson(graphics);

            Assert.IsTrue(graphics.Count > 0);
        }
    }
}