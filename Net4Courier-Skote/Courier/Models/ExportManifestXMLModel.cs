using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
namespace Net4Courier.Models
{
        
    public class ExportManifest
    {
        public Address ShipTo;
        public string OrderDate;
        /* The XmlArrayAttribute changes the XML element name
         from the default of "OrderedItems" to "Items". */
        [XmlArrayAttribute("Items")]
        public OrderedItem[] OrderedItems;
        public decimal SubTotal;
        public decimal ShipCost;
        public decimal TotalCost;
    }

    public class Address
    {
        /* The XmlAttribute instructs the XmlSerializer to serialize the Name
           field as an XML attribute instead of an XML element (the default
           behavior). */
        [XmlAttribute]
        public string Name;
        public string Line1;

        /* Setting the IsNullable property to false instructs the
           XmlSerializer that the XML attribute will not appear if
           the City field is set to a null reference. */
        [XmlElementAttribute(IsNullable = false)]
        public string City;
        public string State;
        public string Zip;
    }
    public class OrderedItem
    {
        public string ItemName;
        public string Description;
        public decimal UnitPrice;
        public int Quantity;
        public decimal LineTotal;

        /* Calculate is a custom method that calculates the price per item,
           and stores the value in a field. */
        public void Calculate()
        {
            LineTotal = UnitPrice * Quantity;
        }
    }

}