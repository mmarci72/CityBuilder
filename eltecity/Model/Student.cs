using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.Model
{
    public class Student
    {
        #region Fields

        private Person _person;

        /// <summary>
        /// How many years did the student studied at the education institute (in years)
        /// </summary>
        private Int32 _studyTime;

        #endregion

        #region Properties

        public Person Person => _person;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct a Student from a Person to be used in education institutions
        /// </summary>
        /// <param name="person">From who</param>
        public Student(Person person)
        {
            _person = person;
            _studyTime = 0;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Increments the counter of studied years
        /// and also graduates,
        /// if said counter reaches the institiution's YearsToGraduate property
        /// </summary>
        /// <param name="from">From which school</param>
        public void UpdateYearly(Education from)
        {
            if (from.YearsToGraduate > _studyTime)
            {
                _studyTime++;
            }
            else
            {
                Graduate(from);
            }
            if (_person.IsRetired())
            {
                from.Withdraw(this);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gives the student it's degree and withdraws it from the education institution
        /// </summary>
        /// <param name="from">From which school</param>
        private void Graduate(Education from)
        {
            _person.Degree = from.Degree;
            _person.Education = null;
            from.Withdraw(this);
        }

        #endregion
    }
}
