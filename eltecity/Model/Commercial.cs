namespace ELTECity.Model
{
	public class Commercial : Workplace
	{
		#region Constructor

		public Commercial()
		{
			string imagePathLow1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/commercial_low_1.png");
			string imagePathLow2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/commercial_low_2.png");
			string imagePathMedium1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/commercial_medium_1.png");
			string imagePathMedium2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/commercial_medium_2.png");
			string imagePathHigh1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/commercial_high_1.png");
			string imagePathHigh2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/commercial_high_2.png");

			_pictureSource = new Uri[,]{
				{
					new Uri(imagePathLow1),
					new Uri(imagePathLow2)
				},
				{
					new Uri(imagePathMedium1),
					new Uri(imagePathMedium2)
				},
				{
					new Uri(imagePathHigh1),
					new Uri(imagePathHigh2)
				}
			};

			Price = 100;
		}

		#endregion

		#region Public methods

		public override IField MakeField()
		{
			return new Commercial();
		}

		#endregion
	}
}