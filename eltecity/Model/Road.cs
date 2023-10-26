using System.Reflection;

namespace ELTECity.Model
{
    public class Road : Service
    {
	    #region Constructor

	    public Road()
	    {
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_line.png");
            _pictureSource = new Uri(imagePath);
            _baseProfit = 2;
			Price = 10;
	    }

	    #endregion

        #region Public methods

		public override IField MakeField()
		{
			return new Road();
		}


		#endregion
	}
}