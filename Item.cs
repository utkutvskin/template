using System;
using System.Xml.Serialization;

namespace CinemaManagementSystem.Items
{
    [Serializable]
    // ---------------------------------------------------------
    // INHERITANCE IMPLEMENTATION: Abstract Base Class
    // ---------------------------------------------------------
    // Introduces the Item hierarchy. Includes Snack and Glass3D for XML serialization.
    [XmlInclude(typeof(Snack))]
    [XmlInclude(typeof(Glass3D))]
    public abstract class Item
    {
        public string Name { get; protected set; }
        public double Price { get; protected set; }

        protected Item() { }

        protected Item(string name, double price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.");

            if (price < 0)
                throw new ArgumentException("Price must be positive.");

            Name = name;
            Price = price;
        }
    }
}
