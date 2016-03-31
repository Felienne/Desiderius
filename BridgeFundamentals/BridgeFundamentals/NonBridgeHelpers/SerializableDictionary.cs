using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sodes.Base
{
	[XmlRoot("dictionary")]
	public class SerializableDictionary<TKey, TValue>
			: Dictionary<TKey, TValue>, IXmlSerializable
	{
		/// <summary>
		/// Needed for deserialization
		/// </summary>
		public SerializableDictionary()			// must be public for WSDL generator
		{
		}

		public SerializableDictionary(IEqualityComparer<TKey> comparer)
			: base(comparer)
		{
		}

		/// <summary>
		/// Needed for serialization
		/// </summary>
		//protected SerializableDictionary(SerializationInfo info, StreamingContext context)
		//  : base(info, context)
		//{
		//}

		#region IXmlSerializable Members

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				reader.ReadStartElement("item");

				reader.ReadStartElement("key");
				TKey key = (TKey)keySerializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadStartElement("value");
				TValue value = (TValue)valueSerializer.Deserialize(reader);
				reader.ReadEndElement();

				this[key] = value;

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

			foreach (TKey key in this.Keys)
			{
				writer.WriteStartElement("item");

				writer.WriteStartElement("key");
				keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				writer.WriteStartElement("value");
				TValue value = this[key];
				valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
		#endregion
	}
}