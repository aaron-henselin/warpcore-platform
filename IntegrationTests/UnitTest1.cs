using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarpCore.Crm;

namespace IntegrationTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var indRepo = new IndividualRepository();
            indRepo.Save(new Individual{FirstName = "Aaron"});
        }
    }
}
