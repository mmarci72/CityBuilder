namespace ELTECity.Model
{
	public class Industrial : Workplace
	{

		#region Constructor

		public Industrial()
		{
			string imagePathLow1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/industrial_low_low.png");
			string imagePathLow2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/industrial_low_high.png");
			string imagePathMedium1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/industrial_medium_low.png");
			string imagePathMedium2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/industrial_medium_high.png");
			string imagePathHigh1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/industrial_high_low.png");
			string imagePathHigh2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/industrial_high_high.png");

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
			return new Industrial();
		}

		#endregion
	}
}