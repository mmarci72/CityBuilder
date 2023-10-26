namespace ELTECity.Model
{
    public class Forest : Service
    {
        #region Fields

        private Int32 _age;

        #endregion

        #region Properties

        public Int32 Age => _age;

        #endregion

        #region Constructor

        public Forest()
        {
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/forest.png");
            _pictureSource = new Uri(imagePath);
            _age = 0;
            Price = 200;
        }

        public Forest(Int32 age)
        {
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/forest.png");
            _pictureSource = new Uri(imagePath);
            _age = age;
            Price = 200;
        }

        #endregion

        #region Public methods

        public override Int32 UpdateMonthly(City city)
        {
            UpdatedMonthlyAlready = true;
            return _age < 10 ? base.UpdateMonthly(city) : 0;
        }

        public override Int32 UpdateYearly(City city)
        {
            _age++;
            UpdatedYearlyAlready = true;
            return 0;
        }

        public override IField MakeField()
        {
            return new Forest();
        }

        #endregion
    }
}