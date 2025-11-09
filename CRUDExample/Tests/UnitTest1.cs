namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Sum()
        {
            //Arrange
            MyMath mm = new MyMath();
            int x = 10, y=5;
            int expected = 15;

            //Act
            int actual = mm.Add(x, y);

            //Assert
            Assert.Equal(expected, actual);
        }
    }
}