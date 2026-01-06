using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CinemaManagementSystem.Exceptions;

namespace CinemaManagementSystem.ContractTypeForEmployee
{
    [Serializable]
    // ---------------------------------------------------------
    // INHERITANCE IMPLEMENTATION: Dynamic Inheritance (Composition)
    // ---------------------------------------------------------
    // Instead of inheriting from Employee (e.g., class PartTimeEmployee : Employee),
    // we use Composition. The Employee class holds a reference to this contract.
    // This allows an Employee to change from PartTime to FullTime at runtime.
    public class PartTimeContract 
    {
        private int _hoursPerWeek;
        private static int MaxHours = 6;

        public int HoursPerWeek
        {
            get => _hoursPerWeek;
            set
            {
                if (value <= 0 || value > MaxHours * 5)
                    throw new ArgumentException("Part-time employee must work between 1 and 30 hours.");
                _hoursPerWeek = value;
            }
        }
        
        [XmlIgnore]
        private Employee _employee;
        [XmlIgnore]
        public Employee Employee => _employee;
        
        private void SetEmployee(Employee employee)
        {
            if(employee == null)
                throw new ArgumentNullException(nameof(employee));
            
            if(employee == _employee)
                throw new DuplicateException(employee, this);

            employee.SetPartTime(this);
            _employee = employee;
        }

        //  Class extent 
        private static List<PartTimeContract> _contracts = new List<PartTimeContract>();
        public static IReadOnlyList<PartTimeContract> Contracts => _contracts.AsReadOnly();

        private static void AddContract(PartTimeContract contract)
        {
            if (contract == null)
                throw new ArgumentException("contract cannot be null");

            _contracts.Add(contract);
        }

        internal void RemoveFromExtent()
        {
            _contracts.Remove(this);
        }
        
        public PartTimeContract() { }

        public PartTimeContract(int hoursPerWeek, Employee employee)
        {
            HoursPerWeek = hoursPerWeek;
            SetEmployee(employee);
            AddContract(this);
        }

        public override string ToString()
        {
            return $"Part-Time Contract ({HoursPerWeek}h/week)";
        }
    }
}
