using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeopuntTests1
{
    public class ArcGISTestClass : Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute
    {
        public ArcGISTestClass(string productId = "ArcGISPro") : base()
        {
            // Install domain wide assembly resolver
            TestResolver.Install(productId);
        }
    }
}
