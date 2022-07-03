using src;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Test
{
    [TestClass]
    public class CircleTest
    {
        [TestMethod]
        public void StandartNumber()
        {
            double result = FigureCalculator.FindAreaOfCircle(5);

            Assert.AreEqual(78.539816, result);
        }

        [TestMethod]
        public void ZeroRadius()
        {
            double result = FigureCalculator.FindAreaOfCircle(0);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void InvalidRadius()
        {
            Assert.ThrowsException<ArgumentException>(() => FigureCalculator.FindAreaOfCircle(-1));
        }
    }
}
