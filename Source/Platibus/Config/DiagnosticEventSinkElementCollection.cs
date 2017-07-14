using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Platibus.Config
{
    /// <summary>
    /// Collection of <see cref="DiagnosticEventSinkElement"/>s
    /// </summary>
    public class DiagnosticEventSinkElementCollection : ConfigurationElementCollection, IEnumerable<DiagnosticEventSinkElement>
    {
        /// <inheritdoc />
        IEnumerator<DiagnosticEventSinkElement> IEnumerable<DiagnosticEventSinkElement>.GetEnumerator()
        {
            return this.OfType<DiagnosticEventSinkElement>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DiagnosticEventSinkElement();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for. </param>
        protected override object GetElementKey(ConfigurationElement element)
        {
            var sendRuleElement = (DiagnosticEventSinkElement) element;
            return sendRuleElement.Name;
        }
    }
}