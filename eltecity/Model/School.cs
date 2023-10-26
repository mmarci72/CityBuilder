namespace ELTECity.Model
{
	public class School : Education
	{
		#region Constructor

		public School()
		{
			_yearsToGraduate = 4;
			_degree = Degree.Middle;
			_capacity = 600;
			_size = (1, 2);
			string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/school.png");
			_pictureSource = new Uri(imagePath);
			Price = 600;
		}

		#endregion

		#region Public methods

		public override IField MakeField()
		{
			return new School();
		}

		#endregion
	}
}