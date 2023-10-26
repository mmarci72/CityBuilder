namespace ELTECity.Model
{
	public class Police : Service
	{
		#region Constructor

		public Police()
		{
			string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/police.png");
			_pictureSource = new Uri(imagePath);
			Price = 400;
		}

		#endregion

		#region Public methods

		public override IField MakeField()
		{
			return new Police();
		}

		#endregion
	}
}