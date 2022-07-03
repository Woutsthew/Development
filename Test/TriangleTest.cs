using src;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{
    [TestClass]
    public class TriangleTest
    {
        [TestMethod]
        public void StandartNumber()
        {
            double result = FigureCalculator.FindAreaOfTriangle(3, 4, 5);

            Assert.AreEqual(6, result);
        }

        [TestMethod]
        public void DefunctTriangle()
        {
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(10, 4, 5));
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(3, 9, 5));
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(3, 4, 8));
        }

        [TestMethod]
        public void InvalidNumber()
        {
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(0, 4, 5));
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(3, 0, 5));
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(3, 4, 0));

            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(-1, 4, 5));
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(3, -1, 5));
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfTriangle(3, 4, -1));
        }

        [TestMethod]
        public void TriangleIsRectangular()
        {
            Assert.IsTrue(FigureCalculator.isRectangular(3, 4, 5));
            Assert.IsTrue(FigureCalculator.isRectangular(8, 10, 6));
        }

        [TestMethod]
        public void TriangleIsNotRectangular()
        {
            Assert.IsFalse(FigureCalculator.isRectangular(3, 2, 5));
            Assert.IsFalse(FigureCalculator.isRectangular(8, 10, 4));
        }
    }
}
