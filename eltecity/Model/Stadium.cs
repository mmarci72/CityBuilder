namespace ELTECity.Model
{
	public class Stadium : Service
	{
		#region Fields

		protected static readonly Int32 _radius;

		#endregion

		#region Constructor

		public Stadium()
		{
			_size = (2, 2);
			string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/stadium.png");
			_pictureSource = new Uri(imagePath);
			Price = 1600;
		}

		#endregion

		#region Public methods

		public override IField MakeField()
		{
			return new Stadium();
		}

		#endregion
	}
}