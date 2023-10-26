using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.Model
{
    public class Education : Service
    {
        #region Fields

        protected List<Student> _students = new List<Student>();
        protected Int32 _yearsToGraduate;
        protected Degree _degree;
        protected Int32 _capacity;

        #endregion

        #region Properties

        public Int32 YearsToGraduate { get { return _yearsToGraduate; } }
        public Degree Degree { get { return _degree; } }

        public List<Student> Students => _students;
    

        #endregion

        #region Public methods

        /// <summary>
        /// Calculates how many people can enroll yearly
        /// </summary>
        /// <returns>Yearly enrollment limit</returns>
        public Int32 EnrollLimit()
        { return _capacity / _yearsToGraduate; }

        /// <summary>
        /// Enrolls a person to the institution by making them a student,
        /// if the capacity allows to or the person is not retired
        /// </summary>
        /// <param name="person">Who to enroll</param>
        /// <returns>The enrollment was successful or not</returns>
        public Boolean Enroll(Person person)
        {
            if (_students.Count < _capacity && !person.IsRetired())
            {
                _students.Add(new Student(person));
                person.Education = this;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Withdraws the student from the institution
        /// </summary>
        /// <param name="student">Who to withdraw</param>
        /// <returns>Withdrawal was successful or not</returns>
        public bool Withdraw(Student student)
        {
            return _students.Remove(student);
        }

        public bool Withdraw(Person person)
        {
	        Student? student = _students.FirstOrDefault(p => p.Person == person);
	        if (student == null)
	        {
		        return false;
	        }
	        return _students.Remove(student);
        }

        public override Int32 UpdateMonthly(City city)
        {
            Int32 profit = 0;
            if (Connected)
            {
                profit = -_baseProfit * _students.Count() / _capacity;
                city.UpkeepCost += profit;
                city.CurrentYearExpense += profit;
                UpdatedMonthlyAlready = true;
            }
            return profit;
        }

        /// <summary>
        /// Increments everybody's StudyTime property by one
        /// </summary>
        public override Int32 UpdateYearly(City city)
        {
            if (Connected)
            {
                for(int i = 0; i < _students.Count(); i++)
                {
                    _students[i].UpdateYearly(this);
                }
                UpdatedYearlyAlready = true;
            }
            return 0;
        }

        public override IField MakeField()
        {
            return new Education();
        }
        #endregion
    }
}