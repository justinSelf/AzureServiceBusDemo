using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using AsciiIt.Web.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace AsciiItTests
{
    [TestClass]
    public class ImageconverterTests
    {
        private static FileStream imageStream;
        private static Stream textStream;

        [TestInitialize]
        public void TestSetup()
        {
            imageStream = new FileStream("Resources\\test.jpeg", FileMode.Open);

            var data = "some random text";
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            textStream = new MemoryStream(buffer);

        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (imageStream != null) imageStream.Dispose();
            if (textStream != null) textStream.Dispose();
        }

        private HttpPostedFileBase GetStubbedPostedFile(string fileName)
        {
            var httpFileStub = MockRepository.GenerateStub<HttpPostedFileBase>();
            httpFileStub.Stub(stub => stub.FileName).Return(fileName);

            return httpFileStub;
        }

        private HttpPostedFileBase GetStubbedPostedFileWithImage(string fileName)
        {
            var httpFileStub = GetStubbedPostedFile(fileName);
            httpFileStub.Stub(stub => stub.InputStream).Return(imageStream);

            return httpFileStub;
        }


        private HttpPostedFileBase GetStubbedPostedFileWithTextFile(string fileName)
        {
            var httpFileStub = GetStubbedPostedFile(fileName);
            httpFileStub.Stub(s => s.InputStream).Return(textStream);

            return httpFileStub;
        }

        [TestMethod]
        public void FileWithoutImageExtensionShouldReturnNull()
        {
            //Arrange
            var converter = new ImageStreamConverter();
            var httpFileStub = GetStubbedPostedFile("someFile.txt");

            //Act
            var result = converter.GetBitmapFromPostedFile(httpFileStub);

            //Assert
            Assert.IsNull(result);

        }

        [TestMethod]
        public void FileNameWithoutImageExtensionAndLotsOfPeriodsShouldNotBeValidAndReturnNull()
        {
            //Arrange
            var converter = new ImageStreamConverter();

            var httpFileStub = GetStubbedPostedFile("some.File.that.isnt.an.image.txt");

            //Act
            var result = converter.GetBitmapFromPostedFile(httpFileStub);

            //Assert
            Assert.IsNull(result);

        }

        [TestMethod]
        public void FileNameWithImageExtensionShouldNotReturnNull()
        {
            //Arrange
            var converter = new ImageStreamConverter();
            var httpFileStub = GetStubbedPostedFileWithImage("someImage.jpg");

            //Act
            var result = converter.GetBitmapFromPostedFile(httpFileStub);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FileNameWithImageExtensionAndLotsOfPeriodsShouldNotReturnNull()
        {
            //Arrange
            var converter = new ImageStreamConverter();
            var httpFileStub = GetStubbedPostedFileWithImage("some.Image.that.will.be.converted.jpg");

            //Act
            var result = converter.GetBitmapFromPostedFile(httpFileStub);

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TextFileWithImageExtensionShouldReturnNull()
        {
            //Arrange
            var converter = new ImageStreamConverter();
            var httpFileStub = GetStubbedPostedFileWithTextFile("notanImage.jpg");

            //Act
            var result = converter.GetBitmapFromPostedFile(httpFileStub);

            //Assert
            Assert.IsNull(result);
        }
    }
}
