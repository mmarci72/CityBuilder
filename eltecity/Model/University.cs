namespace ELTECity.Model
{
	public class University : Education
	{
		#region Constructor

		public University()
		{
			_yearsToGraduate = 3;
			_degree = Degree.High;
			_capacity = 1800;
			_size = (2, 2);
			string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/university.png");
			_pictureSource = new Uri(imagePath);
			Price = 1200;
		}

		public override IField MakeField()
		{
			return new University();
		}

		#endregion
	}
}