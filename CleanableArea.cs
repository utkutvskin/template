using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using CinemaManagementSystem.AssociationClasses;
using CinemaManagementSystem.PersistenceForAllClasses;

namespace CinemaManagementSystem
{
    [Serializable]
    // ---------------------------------------------------------
    // INHERITANCE IMPLEMENTATION: Polymorphism / Standard Inheritance
    // ---------------------------------------------------------
    // These attributes tell the XML Serializer that this Abstract class
    // can actually contain instances of Hall, Floor, or WC.
    // This enables Polymorphic persistence.
    [XmlInclude(typeof(Hall))]
    [XmlInclude(typeof(Floor))]
    [XmlInclude(typeof(WC))] 
    public abstract class CleanableArea : IExtent<CleanableArea>
    {
        private string _description;
        private TimeSpan _periodBetweenCleanings;

        public string Description
        {
            get => _description;
            set
            {
                if(string.IsNullOrWhiteSpace(value)) 
                    throw new ArgumentException("Description cannot be null or empty");
                _description = value;
            }
        }

        public TimeSpan PeriodBetweenCleanings
        {
            get => _periodBetweenCleanings;
            set
            {
                // Validation to ensure logical consistency
                if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1))
                    throw new ArgumentException("Time must be between 00:00 and 23:59.");
                _periodBetweenCleanings = value;
            }
        }
        
        // ---------------------------------------------------------
        // INHERITANCE: Shared Logic
        // ---------------------------------------------------------
        // This static list manages all instances of Hall, Floor, and WC in a single polymorphic list.
        [XmlIgnore]
        private static List<CleanableArea> _areas = new();
        [XmlIgnore]
        public static IReadOnlyList<CleanableArea> Areas => _areas.AsReadOnly();

        protected void RegisterArea(CleanableArea area)
        {
            _areas.Add(area);
        }

        // Association: CleanableArea - CleanerAssignment
        [XmlIgnore]
        private List<CleanerAssignment> _cleanerAssignments = new();

        [XmlIgnore]
        public IReadOnlyCollection<CleanerAssignment> CleanerAssignments => _cleanerAssignments.AsReadOnly();

        internal void AddCleanerAssignmentInternal(CleanerAssignment assignment)
        {
            if (assignment == null) throw new ArgumentException("assignment cannot be null");
            _cleanerAssignments.Add(assignment);
        }

        internal void RemoveCleanerAssignmentInternal(CleanerAssignment assignment)
        {
            if (assignment == null) throw new ArgumentException("assignment cannot be null");
            _cleanerAssignments.Remove(assignment);
        }

        // ---------------------------------------------------------
        // INHERITANCE: Derived Attribute
        // ---------------------------------------------------------
        // This property is calculated differently based on the data of the specific instance,
        // but the logic is shared across all children.
        [XmlIgnore]
        public bool IsNeedToBeCleaned
        {
            get
            {
                if (_cleanerAssignments.Count == 0) return true;

                DateTime lastCleaning = _cleanerAssignments.Max(a => a.CleaningDateTime);
                return DateTime.Now - lastCleaning > PeriodBetweenCleanings;
            }
        }

        public static List<CleanableArea> GenerateListOfAreaToClean()
        {
            return _areas.Where(a => a.IsNeedToBeCleaned).ToList();
        }

        // Constructors
        protected CleanableArea() {}
        
        // ---------------------------------------------------------
        // INHERITANCE: Base Constructor
        // ---------------------------------------------------------
        // Children classes (Hall, Floor, WC) call this to initialize common state.
        protected CleanableArea(string description, TimeSpan periodBetweenCleanings)
        {
            Description = description;
            PeriodBetweenCleanings = periodBetweenCleanings;
            RegisterArea(this);
        }

        // ---------------------------------------------------------
        // INHERITANCE: Persistence Strategy
        // ---------------------------------------------------------
        // Saves/Loads all types of areas (Hall, Floor, WC) into a single file using polymorphism.
        public static void Save(string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<CleanableArea>));
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            serializer.Serialize(fs, _areas);
        }

        public static void Load(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var serializer = new XmlSerializer(typeof(List<CleanableArea>));
            using (StreamReader reader = new StreamReader(filePath))
            {
                var loaded = (List<CleanableArea>)serializer.Deserialize(reader);
                _areas = loaded ?? new List<CleanableArea>();
            }
        }

        public List<CleanableArea> GetExtent() => _areas;
        public void ReplaceExtent(List<CleanableArea> newExtent) => _areas = newExtent;
    }
}
