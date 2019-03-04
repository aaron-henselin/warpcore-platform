using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarpCore.Cms;
using WarpCore.Cms.Routing;
using WarpCore.Cms.Sites;
using WarpCore.DbEngines.AzureStorage;
using WarpCore.Platform.DataAnnotations.Expressions;
using WarpCore.Platform.Kernel;
namespace IntegrationTests
{
    //[TestClass]
    //public class UnitTest1
    //{
    //    private Site SetupTestSite()
    //    {
    //        var siteRepo = new SiteRepository();
    //        var newSite = new Site
    //        {
    //            Name = "WarpCore Demo"
    //        };
    //        siteRepo.Save(newSite);

    //        var homePage = new CmsPage
    //        {
    //            Name = "Homepage",
    //            SiteId = newSite.ContentId
    //        };
    //        var aboutUs = new CmsPage
    //        {
    //            Name = "About Us",
    //            SiteId = newSite.ContentId
    //        };
    //        var contactUs = new CmsPage
    //        {
    //            Name = "Contact Us",
    //            SiteId = newSite.ContentId
    //        };

    //        var pageRepository = new CmsPageRepository();
    //        pageRepository.Save(homePage, SitemapRelativePosition.Root);
    //        pageRepository.Save(aboutUs, SitemapRelativePosition.Root);
    //        pageRepository.Save(contactUs, SitemapRelativePosition.Root);
    //        newSite.HomepageId = homePage.ContentId;
    //        siteRepo.Save(newSite);

    //        var subPage1 = new CmsPage
    //        {
    //            Name = "Subpage 1",
    //            SiteId = newSite.ContentId
    //        };
    //        pageRepository.Save(subPage1, new PageRelativePosition { ParentPageId = homePage.ContentId });

    //        var subPage0 = new CmsPage
    //        {
    //            Name = "Subpage 0",
    //            SiteId = newSite.ContentId
    //        };
    //        pageRepository.Save(subPage0, new PageRelativePosition { ParentPageId = homePage.ContentId, BeforePageId = subPage1.ContentId });

    //        return newSite;
    //    }

    //    private static void AssertUrlIsRoutable(params string[] uris)
    //    {
    //        foreach (var uri in uris)
    //        {
    //            var success = CmsRoutes.Current.TryResolveRoute(new Uri(uri, UriKind.Absolute), out _);
    //            Assert.IsTrue(success);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestMethod1()
    //    {
    //        Dependency.Register<ICosmosOrm>(typeof(InMemoryDb));

    //        var newSite = SetupTestSite();


    //        var liveSitemapBefore = SitemapBuilder.BuildSitemap(newSite, ContentEnvironment.Live, SitemapBuilderFilters.All);
    //        Assert.AreEqual(0, liveSitemapBefore.ChildNodes.Count);

    //        PublishingShortcuts.PublishSite(newSite);


    //        var structure = SiteStructureMapBuilder.BuildStructureMap(newSite);
    //        Assert.AreEqual(3, structure.ChildNodes.Count);


    //        //Assert.AreEqual(subPage0.ContentId, structure.ChildNodes.ElementAt(0).ChildNodes.ElementAt(0).PageId);
    //        //Assert.AreEqual(subPage1.ContentId, structure.ChildNodes.ElementAt(0).ChildNodes.ElementAt(1).PageId);


    //        var liveSitemap = SitemapBuilder.BuildSitemap(newSite, ContentEnvironment.Live, SitemapBuilderFilters.All);
    //        Assert.AreEqual(3, liveSitemap.ChildNodes.Count);


    //        //pageRepository.Move(subPage0,SitemapRelativePosition.Root);

    //        AssertUrlIsRoutable(
    //            "http://www.google.com",
    //            "http://www.google.com/",
    //            "http://www.google.com/homepage/subpage-0",
    //            "http://www.google.com/homepage/subpage-0/");

    //        var lastPage = liveSitemap.ChildNodes.Last().Page;


    //        var uriBuilder = new CmsUriBuilder(
    //                new UriBuilderContext
    //                {
    //                    AbsolutePath = new Uri("/",UriKind.Relative),
    //                    Authority = "www.google.com",
    //                    IsSsl = false
    //                }
    //            );

    //        uriBuilder.CreateUri(lastPage,UriSettings.Default,null);

    //    }
    //}

    [TestClass]
    public class BooleanExpressionEvaluatorTests
    {
        [TestClass]
        public class Evaluate
        {
            private BooleanExpressionParser _parser = new BooleanExpressionParser();
            private BooleanExpressionTokenReader _tokenReader = new BooleanExpressionTokenReader();
         


            private class Scope : Dictionary<string, object>, IVariableScope
            {
                public object GetValue(string key)
                {
                    return this[key];
                }
            }

            private bool Run(string expression)
            {
                var scope = new Scope {{"one", 1}, {"two", 2}};
                Evaluator evaluator = new Evaluator(scope);
                var tokens = _parser.ReadBooleanExpressionTokens(expression);
                var compiled = _tokenReader.CreateBooleanExpressionFromTokens(tokens);
                return evaluator.Evaluate(compiled);
            }


            [TestMethod]
            public void EvaluatesEquals()
            {
                var result = Run("one == 1");
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void EvaluatesNotEquals()
            {

                var result = Run("one != two");
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void EvaluatesGreaterThan()
            {
                var result = Run("two > one");
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void EvaluatesLessThan()
            {
                var result = Run("one < two");
                Assert.IsTrue(result);
            }


            [TestMethod]
            public void EvaluatesLogicalOrBooleanBinaryExpressions()
            {
                var result = Run("one < two || two < one");
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void EvaluatesLogicalAndBooleanBinaryExpressions()
            {
                var result = Run("one > two && 1 < 2");
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void EvaluatesParenExpressions()
            {
                var result = Run("1 < 2 && (1 > 2 || 1 < 2)");
                Assert.IsTrue(result);
            }

        }
    }

    [TestClass]
    public class BooleanExpressionParserTests
    {
        

        [TestClass]
        public class ReadTokens
        {
            private BooleanExpressionParser _parser;
            private BooleanExpressionTokenReader _tokenReader;

            public ReadTokens()
            {
                _parser = new BooleanExpressionParser();
                _tokenReader = new BooleanExpressionTokenReader();
            }

            private void AssertSameMeaningAfterCompile(string originalExpression)
            {
                
                var tokens = _parser.ReadBooleanExpressionTokens(originalExpression);
                var compiled = _tokenReader.CreateBooleanExpressionFromTokens(tokens);
                Assert.AreEqual(originalExpression,compiled.ToString());
            }

            [TestMethod]
            public void ReadsEquals()
            {
                AssertSameMeaningAfterCompile("a == b");
            }

            [TestMethod]
            public void ReadsNotEquals()
            {
                AssertSameMeaningAfterCompile("a != b");
            }

            [TestMethod]
            public void ReadsParens()
            {
                AssertSameMeaningAfterCompile("(a == (b == c))");
            }

            [TestMethod]
            public void ReadsBooleanBinaryExpressions()
            {
                AssertSameMeaningAfterCompile("a || b");
            }

            [TestMethod]
            public void ReadsBooleanBinaryExpressionsWhenNotExplicitGrouping()
            {
                AssertSameMeaningAfterCompile("a || b || c && d");
            }

            [TestMethod]
            public void ReadsComplexInnnerExpressions()
            {
                AssertSameMeaningAfterCompile("(a == (b == (c || d && e < 2.1)) || f && x > 2)");
            }
        }



    }
}
