using System.Web;
using AsciiIt.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace AsciiItTests
{
    [TestClass]
    public class ImageValidatorTests
    {
        [TestMethod]
        public void FileWithoutImageExtensionShouldNotBeValid()
        {
            //Arrange
            var validator = new ImageValidator();

            var httpFileStub = MockRepository.GenerateStub<HttpPostedFileBase>();
            httpFileStub.Stub(stub => stub.FileName).Return("someFile.txt");

            //Act

            var result = validator.IsValidPostedImage(httpFileStub);

            //Assert
            Assert.IsFalse(result);

        }

        [TestMethod]
        public void FileWithoutImageExtensionAndLotsOfPeriodsShouldNotBeValid()
        {
            //Arrange
            var validator = new ImageValidator();

            var httpFileStub = MockRepository.GenerateStub<HttpPostedFileBase>();
            httpFileStub.Stub(stub => stub.FileName).Return("some.File.that.isnt.an.image.txt");

            //Act

            var result = validator.IsValidPostedImage(httpFileStub);

            //Assert
            Assert.IsFalse(result);

        }

        [TestMethod]
        public void FileWithImageExtensionShouldBeValid()
        {
            //Arrange
            var validator = new ImageValidator();

            var httpFileStub = MockRepository.GenerateStub<HttpPostedFileBase>();
            httpFileStub.Stub(stub => stub.FileName).Return("someImage.jpg");

            //Act

            var result = validator.IsValidPostedImage(httpFileStub);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FileWithImageExtensionAndLotsOfPeriodsShouldBeValid()
        {
            //Arrange
            var validator = new ImageValidator();

            var httpFileStub = MockRepository.GenerateStub<HttpPostedFileBase>();
            httpFileStub.Stub(stub => stub.FileName).Return("some.Image.that.will.be.converted.jpg");

            //Act

            var result = validator.IsValidPostedImage(httpFileStub);

            //Assert
            Assert.IsTrue(result);
        }
    }
}
