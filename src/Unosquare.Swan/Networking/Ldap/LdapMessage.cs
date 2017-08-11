﻿#if !UWP
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Unosquare.Swan.Networking.Ldap
{

    /// <summary>
    ///     This class contains different methods to manage Collections.
    /// </summary>
    public class CollectionSupport : CollectionBase
    {
        /// <summary>
        ///     Adds an specified element to the collection.
        /// </summary>
        /// <param name="element">The element to be added.</param>
        /// <returns>Returns true if the element was successfuly added. Otherwise returns false.</returns>
        public virtual bool Add(object element)
        {
            return List.Add(element) != -1;
        }

        /// <summary>
        ///     Adds all the elements contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(ICollection collection)
        {
            var result = false;
            if (collection != null)
            {
                var tempEnumerator = new ArrayList(collection).GetEnumerator();
                while (tempEnumerator.MoveNext())
                {
                    if (tempEnumerator.Current != null)
                        result = Add(tempEnumerator.Current);
                }
            }
            return result;
        }

        /// <summary>
        ///     Adds all the elements contained in the specified support class collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(CollectionSupport collection)
        {
            return AddAll((ICollection)collection);
        }

        /// <summary>
        ///     Verifies if the specified element is contained into the collection.
        /// </summary>
        /// <param name="element"> The element that will be verified.</param>
        /// <returns>Returns true if the element is contained in the collection. Otherwise returns false.</returns>
        public virtual bool Contains(object element)
        {
            return List.Contains(element);
        }

        /// <summary>
        ///     Verifies if all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
        public virtual bool ContainsAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = new ArrayList(collection).GetEnumerator();
            while (tempEnumerator.MoveNext())
                if (!(result = Contains(tempEnumerator.Current)))
                    break;
            return result;
        }

        /// <summary>
        ///     Verifies if all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
        public virtual bool ContainsAll(CollectionSupport collection)
        {
            return ContainsAll((ICollection)collection);
        }

        /// <summary>
        ///     Verifies if the collection is empty.
        /// </summary>
        /// <returns>Returns true if the collection is empty. Otherwise returns false.</returns>
        public virtual bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        ///     Removes an specified element from the collection.
        /// </summary>
        /// <param name="element">The element to be removed.</param>
        /// <returns>Returns true if the element was successfuly removed. Otherwise returns false.</returns>
        public virtual bool Remove(object element)
        {
            var result = false;
            if (Contains(element))
            {
                List.Remove(element);
                result = true;
            }
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
        public virtual bool RemoveAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = new ArrayList(collection).GetEnumerator();
            while (tempEnumerator.MoveNext())
            {
                if (Contains(tempEnumerator.Current))
                    result = Remove(tempEnumerator.Current);
            }
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
        public virtual bool RemoveAll(CollectionSupport collection)
        {
            return RemoveAll((ICollection)collection);
        }

        /// <summary>
        ///     Removes all the elements that aren't contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RetainAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = GetEnumerator();
            var tempCollection = new CollectionSupport();
            tempCollection.AddAll(collection);
            while (tempEnumerator.MoveNext())
                if (!tempCollection.Contains(tempEnumerator.Current))
                {
                    result = Remove(tempEnumerator.Current);

                    if (result)
                    {
                        tempEnumerator = GetEnumerator();
                    }
                }
            return result;
        }

        /// <summary>
        ///     Removes all the elements that aren't contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RetainAll(CollectionSupport collection)
        {
            return RetainAll((ICollection)collection);
        }

        /// <summary>
        ///     Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <returns>The array containing all the elements of the collection</returns>
        public virtual object[] ToArray()
        {
            var index = 0;
            var objects = new object[Count];
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                objects[index++] = tempEnumerator.Current;
            return objects;
        }

        /// <summary>
        ///     Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <param name="objects">The array into which the elements of the collection will be stored.</param>
        /// <returns>The array containing all the elements of the collection.</returns>
        public virtual object[] ToArray(object[] objects)
        {
            var index = 0;
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                objects[index++] = tempEnumerator.Current;
            return objects;
        }

        /// <summary>
        ///     Creates a CollectionSupport object with the contents specified in array.
        /// </summary>
        /// <param name="array">The array containing the elements used to populate the new CollectionSupport object.</param>
        /// <returns>A CollectionSupport object populated with the contents of array.</returns>
        public static CollectionSupport ToCollectionSupport(object[] array)
        {
            var tempCollectionSupport = new CollectionSupport();
            tempCollectionSupport.AddAll(array);
            return tempCollectionSupport;
        }
    }

    /// <summary>
    ///     This class manages a set of elements.
    /// </summary>
    public class SetSupport : ArrayList
    {
        /// <summary>
        ///     Creates a new set.
        /// </summary>
        public SetSupport()
        {
        }

        /// <summary>
        ///     Creates a new set initialized with System.Collections.ICollection object
        /// </summary>
        /// <param name="collection">System.Collections.ICollection object to initialize the set object</param>
        public SetSupport(ICollection collection) : base(collection)
        {
        }

        /// <summary>
        ///     Creates a new set initialized with a specific capacity.
        /// </summary>
        /// <param name="capacity">value to set the capacity of the set object</param>
        public SetSupport(int capacity) : base(capacity)
        {
        }

        /// <summary>
        ///     Adds an element to the set.
        /// </summary>
        /// <param name="objectToAdd">The object to be added.</param>
        /// <returns>True if the object was added, false otherwise.</returns>
        public new virtual bool Add(object objectToAdd)
        {
            if (Contains(objectToAdd))
                return false;
            base.Add(objectToAdd);
            return true;
        }

        /// <summary>
        ///     Adds all the elements contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(ICollection collection)
        {
            var result = false;
            if (collection != null)
            {
                var tempEnumerator = new ArrayList(collection).GetEnumerator();
                while (tempEnumerator.MoveNext())
                {
                    if (tempEnumerator.Current != null)
                        result = Add(tempEnumerator.Current);
                }
            }
            return result;
        }

        /// <summary>
        ///     Adds all the elements contained in the specified support class collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be added.</param>
        /// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
        public virtual bool AddAll(CollectionSupport collection)
        {
            return AddAll((ICollection)collection);
        }

        /// <summary>
        ///     Verifies that all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>True if the collection contains all the given elements.</returns>
        public virtual bool ContainsAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = collection.GetEnumerator();
            while (tempEnumerator.MoveNext())
                if (!(result = Contains(tempEnumerator.Current)))
                    break;
            return result;
        }

        /// <summary>
        ///     Verifies if all the elements of the specified collection are contained into the current collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be verified.</param>
        /// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
        public virtual bool ContainsAll(CollectionSupport collection)
        {
            return ContainsAll((ICollection)collection);
        }

        /// <summary>
        ///     Verifies if the collection is empty.
        /// </summary>
        /// <returns>True if the collection is empty, false otherwise.</returns>
        public virtual bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        ///     Removes an element from the set.
        /// </summary>
        /// <param name="elementToRemove">The element to be removed.</param>
        /// <returns>True if the element was removed.</returns>
        public new virtual bool Remove(object elementToRemove)
        {
            var result = false;
            if (Contains(elementToRemove))
                result = true;
            base.Remove(elementToRemove);
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>True if all the elements were successfuly removed, false otherwise.</returns>
        public virtual bool RemoveAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = collection.GetEnumerator();
            while (tempEnumerator.MoveNext())
            {
                if (result == false && Contains(tempEnumerator.Current))
                    result = true;
                Remove(tempEnumerator.Current);
            }
            return result;
        }

        /// <summary>
        ///     Removes all the elements contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to extract the elements that will be removed.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RemoveAll(CollectionSupport collection)
        {
            return RemoveAll((ICollection)collection);
        }

        /// <summary>
        ///     Removes all the elements that aren't contained in the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>True if all the elements were successfully removed, false otherwise.</returns>
        public virtual bool RetainAll(ICollection collection)
        {
            var result = false;
            var tempEnumerator = collection.GetEnumerator();
            var tempSet = (SetSupport)collection;
            while (tempEnumerator.MoveNext())
                if (!tempSet.Contains(tempEnumerator.Current))
                {
                    result = Remove(tempEnumerator.Current);
                    tempEnumerator = GetEnumerator();
                }
            return result;
        }

        /// <summary>
        ///     Removes all the elements that aren't contained into the specified collection.
        /// </summary>
        /// <param name="collection">The collection used to verify the elements that will be retained.</param>
        /// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
        public virtual bool RetainAll(CollectionSupport collection)
        {
            return RetainAll((ICollection)collection);
        }

        /// <summary>
        ///     Obtains an array containing all the elements of the collection.
        /// </summary>
        /// <returns>The array containing all the elements of the collection.</returns>
        public new virtual object[] ToArray()
        {
            var index = 0;
            var tempObject = new object[Count];
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                tempObject[index++] = tempEnumerator.Current;
            return tempObject;
        }

        /// <summary>
        ///     Obtains an array containing all the elements in the collection.
        /// </summary>
        /// <param name="objects">The array into which the elements of the collection will be stored.</param>
        /// <returns>The array containing all the elements of the collection.</returns>
        public virtual object[] ToArray(object[] objects)
        {
            var index = 0;
            var tempEnumerator = GetEnumerator();
            while (tempEnumerator.MoveNext())
                objects[index++] = tempEnumerator.Current;
            return objects;
        }
    }

    /// <summary>
    ///     This  class  extends the AbstractSet and Implements the Set
    ///     so that it can be used to maintain a list of currently
    ///     registered extended responses.
    /// </summary>
    public class RespExtensionSet : SetSupport
    {
        /// <summary>
        ///     Returns the number of extensions in this set.
        /// </summary>
        /// <returns>
        ///     number of extensions in this set.
        /// </returns>
        public override int Count
        {
            get { return map.Count; }
        }

        private readonly Hashtable map;

        public RespExtensionSet()
        {
            map = new Hashtable();
        }
        
        public void registerResponseExtension(string oid, Type extClass)
        {
            lock (this)
            {
                if (!map.ContainsKey(oid))
                {
                    map.Add(oid, extClass);
                }
            }
        }

        /// <summary>
        ///     Returns an iterator over the responses in this set.  The responses
        ///     returned from this iterator are not in any particular order.
        /// </summary>
        /// <returns>
        ///     iterator over the responses in this set
        /// </returns>
        public override IEnumerator GetEnumerator()
        {
            return map.Values.GetEnumerator();
        }

        /* Searches the list of registered responses for a mathcing response.  We
        * search using the OID string.  If a match is found we return the
        * Class name that was provided to us on registration.
        */

        public Type findResponseExtension(string searchOID)
        {
            lock (this)
            {
                if (map.ContainsKey(searchOID))
                {
                    return (Type)map[searchOID];
                }
                /* The requested extension does not have a registered response class */
                return null;
            }
        }
    }

    /// <summary>
    ///     Represents the Ldap Abandon Request.
    ///     <pre>
    ///         AbandonRequest ::= [APPLICATION 16] MessageID
    ///     </pre>
    /// </summary>
    internal class RfcAbandonRequest : RfcMessageID, RfcRequest
    {
        /// <summary> Constructs an RfcAbandonRequest</summary>
        public RfcAbandonRequest(int msgId) : base(msgId)
        {
        }
        
        /// <summary>
        ///     Override getIdentifier to return an application-wide id.
        ///     <pre>
        ///         ID = CLASS: APPLICATION, FORM: CONSTRUCTED, TAG: 16. (0x50)
        ///     </pre>
        /// </summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, false, LdapMessage.ABANDON_REQUEST);
        }

        public RfcRequest dupRequest(string base_Renamed, string filter, bool reference)
        {
            throw new LdapException(ExceptionMessages.NO_DUP_REQUEST, new object[] { "Abandon" },
                LdapException.Ldap_NOT_SUPPORTED, null);
        }

        public string getRequestDN()
        {
            return null;
        }
    }

    /// <summary>
    ///     Represents an Ldap Abandon Request
    /// </summary>
    /// <seealso cref="LdapConnection.SendRequest">
    /// </seealso>
    /*
     *       AbandonRequest ::= [APPLICATION 16] MessageID
     */
    public class LdapAbandonRequest : LdapMessage
    {
        /// <summary>
        ///     Construct an Ldap Abandon Request.
        /// </summary>
        /// <param name="id">
        ///     The ID of the operation to abandon.
        /// </param>
        /// <param name="cont">
        ///     Any controls that apply to the abandon request
        ///     or null if none.
        /// </param>
        public LdapAbandonRequest(int id, LdapControl[] cont) : base(ABANDON_REQUEST, new RfcAbandonRequest(id), cont)
        {
        }
    }

    /// <summary>
    ///     Represents an LdapResult.
    ///     <pre>
    ///         LdapResult ::= SEQUENCE {
    ///         resultCode      ENUMERATED {
    ///         success                      (0),
    ///         operationsError              (1),
    ///         protocolError                (2),
    ///         timeLimitExceeded            (3),
    ///         sizeLimitExceeded            (4),
    ///         compareFalse                 (5),
    ///         compareTrue                  (6),
    ///         authMethodNotSupported       (7),
    ///         strongAuthRequired           (8),
    ///         -- 9 reserved --
    ///         referral                     (10),  -- new
    ///         adminLimitExceeded           (11),  -- new
    ///         unavailableCriticalExtension (12),  -- new
    ///         confidentialityRequired      (13),  -- new
    ///         saslBindInProgress           (14),  -- new
    ///         noSuchAttribute              (16),
    ///         undefinedAttributeType       (17),
    ///         inappropriateMatching        (18),
    ///         constraintViolation          (19),
    ///         attributeOrValueExists       (20),
    ///         invalidAttributeSyntax       (21),
    ///         -- 22-31 unused --
    ///         noSuchObject                 (32),
    ///         aliasProblem                 (33),
    ///         invalidDNSyntax              (34),
    ///         -- 35 reserved for undefined isLeaf --
    ///         aliasDereferencingProblem    (36),
    ///         -- 37-47 unused --
    ///         inappropriateAuthentication  (48),
    ///         invalidCredentials           (49),
    ///         insufficientAccessRights     (50),
    ///         busy                         (51),
    ///         unavailable                  (52),
    ///         unwillingToPerform           (53),
    ///         loopDetect                   (54),
    ///         -- 55-63 unused --
    ///         namingViolation              (64),
    ///         objectClassViolation         (65),
    ///         notAllowedOnNonLeaf          (66),
    ///         notAllowedOnRDN              (67),
    ///         entryAlreadyExists           (68),
    ///         objectClassModsProhibited    (69),
    ///         -- 70 reserved for CLdap --
    ///         affectsMultipleDSAs          (71), -- new
    ///         -- 72-79 unused --
    ///         other                        (80) },
    ///         -- 81-90 reserved for APIs --
    ///         matchedDN       LdapDN,
    ///         errorMessage    LdapString,
    ///         referral        [3] Referral OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcLdapResult : Asn1Sequence, RfcResponse
    {
        /// <summary> Context-specific TAG for optional Referral.</summary>
        public const int REFERRAL = 3;

        //*************************************************************************
        // Constructors for RfcLdapResult
        //*************************************************************************

        /// <summary>
        ///     Constructs an RfcLdapResult from parameters
        /// </summary>
        /// <param name="resultCode">
        ///     the result code of the operation
        /// </param>
        /// <param name="matchedDN">
        ///     the matched DN returned from the server
        /// </param>
        /// <param name="errorMessage">
        ///     the diagnostic message returned from the server
        /// </param>
        public RfcLdapResult(Asn1Enumerated resultCode, RfcLdapDN matchedDN, RfcLdapString errorMessage)
            : this(resultCode, matchedDN, errorMessage, null)
        {
        }

        /// <summary>
        ///     Constructs an RfcLdapResult from parameters
        /// </summary>
        /// <param name="resultCode">
        ///     the result code of the operation
        /// </param>
        /// <param name="matchedDN">
        ///     the matched DN returned from the server
        /// </param>
        /// <param name="errorMessage">
        ///     the diagnostic message returned from the server
        /// </param>
        /// <param name="referral">
        ///     the referral(s) returned by the server
        /// </param>
        public RfcLdapResult(Asn1Enumerated resultCode, RfcLdapDN matchedDN, RfcLdapString errorMessage,
            Asn1SequenceOf referral) : base(4)
        {
            Add(resultCode);
            Add(matchedDN);
            Add(errorMessage);
            if (referral != null)
                Add(referral);
        }

        /// <summary> Constructs an RfcLdapResult from the inputstream</summary>
        
        public RfcLdapResult(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Size() > 3)
            {
                var obj = (Asn1Tagged)Get(3);
                var id = obj.GetIdentifier();
                if (id.Tag == REFERRAL)
                {
                    var content = ((Asn1OctetString)obj.taggedValue()).ByteValue();
                    var bais = new MemoryStream(content.ToByteArray());
                    Set(3, new Asn1SequenceOf(dec, bais, content.Length));
                }
            }
        }

        //*************************************************************************
        // Accessors
        //*************************************************************************

        /// <summary>
        ///     Returns the result code from the server
        /// </summary>
        /// <returns>
        ///     the result code
        /// </returns>
        public Asn1Enumerated getResultCode()
        {
            return (Asn1Enumerated)Get(0);
        }

        /// <summary>
        ///     Returns the matched DN from the server
        /// </summary>
        /// <returns>
        ///     the matched DN
        /// </returns>
        public RfcLdapDN getMatchedDN()
        {
            return new RfcLdapDN(((Asn1OctetString)Get(1)).ByteValue());
        }

        /// <summary>
        ///     Returns the error message from the server
        /// </summary>
        /// <returns>
        ///     the server error message
        /// </returns>
        public RfcLdapString getErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString)Get(2)).ByteValue());
        }

        /// <summary>
        ///     Returns the referral(s) from the server
        /// </summary>
        /// <returns>
        ///     the referral(s)
        /// </returns>
        public Asn1SequenceOf getReferral()
        {
            return Size() > 3 ? (Asn1SequenceOf)Get(3) : null;
        }
    }

    /// <summary>
    ///     Represents an Ldap Search Result Done Response.
    ///     <pre>
    ///         SearchResultDone ::= [APPLICATION 5] LdapResult
    ///     </pre>
    /// </summary>
    public class RfcSearchResultDone : RfcLdapResult
    {
        //*************************************************************************
        // Constructors for SearchResultDone
        //*************************************************************************

        /// <summary> Decode a search result done from the input stream.</summary>
        
        public RfcSearchResultDone(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
        }

        /// <summary>
        ///     Constructs an RfcSearchResultDone from parameters.
        /// </summary>
        /// <param name="resultCode">
        ///     the result code of the operation
        /// </param>
        /// <param name="matchedDN">
        ///     the matched DN returned from the server
        /// </param>
        /// <param name="errorMessage">
        ///     the diagnostic message returned from the server
        /// </param>
        /// <param name="referral">
        ///     the referral(s) returned by the server
        /// </param>
        public RfcSearchResultDone(Asn1Enumerated resultCode, RfcLdapDN matchedDN, RfcLdapString errorMessage,
            Asn1SequenceOf referral) : base(resultCode, matchedDN, errorMessage, referral)
        {
        }

        //*************************************************************************
        // Accessors
        //*************************************************************************

        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.SEARCH_RESULT);
        }
    }

    /// <summary>
    ///     Represents an Ldap Search Result Entry.
    ///     <pre>
    ///         SearchResultEntry ::= [APPLICATION 4] SEQUENCE {
    ///         objectName      LdapDN,
    ///         attributes      PartialAttributeList }
    ///     </pre>
    /// </summary>
    public class RfcSearchResultEntry : Asn1Sequence
    {
        /// <summary> </summary>
        public virtual Asn1OctetString ObjectName
        {
            get { return (Asn1OctetString)Get(0); }
        }

        /// <summary> </summary>
        public virtual Asn1Sequence Attributes
        {
            get { return (Asn1Sequence)Get(1); }
        }
        
        /// <summary>
        ///     The only time a client will create a SearchResultEntry is when it is
        ///     decoding it from an InputStream
        /// </summary>
        public RfcSearchResultEntry(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            // Decode objectName
            //      set(0, new RfcLdapDN(((Asn1OctetString)get(0)).stringValue()));

            // Create PartitalAttributeList. This does not need to be decoded, only
            // typecast.
            //      set(1, new PartitalAttributeList());
        }
        
        /// <summary> Override getIdentifier to return an application-wide id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.APPLICATION, true, LdapMessage.SEARCH_RESPONSE);
        }
    }

    /// <summary>
    ///     Represents an Ldap Message ID.
    ///     <pre>
    ///         MessageID ::= INTEGER (0 .. maxInt)
    ///         maxInt INTEGER ::= 2147483647 -- (2^^31 - 1) --
    ///         Note: The creation of a MessageID should be hidden within the creation of
    ///         an RfcLdapMessage. The MessageID needs to be in sequence, and has an
    ///         upper and lower limit. There is never a case when a user should be
    ///         able to specify the MessageID for an RfcLdapMessage. The MessageID()
    ///         class should be package protected. (So the MessageID value isn't
    ///         arbitrarily run up.)
    ///     </pre>
    /// </summary>
    internal class RfcMessageID : Asn1Integer
    {
        /// <summary>
        ///     Increments the message number atomically
        /// </summary>
        /// <returns>
        ///     the new message number
        /// </returns>
        private static int MessageID
        {
            get
            {
                lock (lock_Renamed)
                {
                    return messageID < int.MaxValue ? ++messageID : (messageID = 1);
                }
            }
        }

        private static int messageID;
        private static readonly object lock_Renamed;

        /// <summary>
        ///     Creates a MessageID with an auto incremented Asn1Integer value.
        ///     Bounds: (0 .. 2,147,483,647) (2^^31 - 1 or Integer.MAX_VALUE)
        ///     MessageID zero is never used in this implementation.  Always
        ///     start the messages with one.
        /// </summary>
        protected internal RfcMessageID() : base(MessageID)
        {
        }

        /// <summary> Creates a MessageID with a specified int value.</summary>
        protected internal RfcMessageID(int i) : base(i)
        {
        }

        static RfcMessageID()
        {
            lock_Renamed = new object();
        }
    }

    /// <summary>
    ///     Represents Ldap Contreols.
    ///     <pre>
    ///         Controls ::= SEQUENCE OF Control
    ///     </pre>
    /// </summary>
    public class RfcControls : Asn1SequenceOf
    {
        /// <summary> Controls context specific tag</summary>
        public const int CONTROLS = 0;
        
        /// <summary>
        ///     Constructs a Controls object. This constructor is used in combination
        ///     with the add() method to construct a set of Controls to send to the
        ///     server.
        /// </summary>
        public RfcControls() : base(5)
        {
        }

        /// <summary> Constructs a Controls object by decoding it from an InputStream.</summary>
        
        public RfcControls(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            // Convert each SEQUENCE element to a Control
            for (var i = 0; i < Size(); i++)
            {
                var tempControl = new RfcControl((Asn1Sequence)Get(i));
                Set(i, tempControl);
            }
        }

        //*************************************************************************
        // Mutators
        //*************************************************************************

        /// <summary> Override add() of Asn1SequenceOf to only accept a Control type.</summary>
        public void add(RfcControl control)
        {
            base.Add(control);
        }

        /// <summary> Override set() of Asn1SequenceOf to only accept a Control type.</summary>
        public void Set(int index, RfcControl control)
        {
            base.Set(index, control);
        }

        //*************************************************************************
        // Accessors
        //*************************************************************************

        /// <summary> Override getIdentifier to return a context specific id.</summary>
        public override Asn1Identifier GetIdentifier()
        {
            return new Asn1Identifier(Asn1Identifier.CONTEXT, true, CONTROLS);
        }
    }

    /// <summary>
    ///     This interface represents RfcLdapMessages that contain a response from a
    ///     server.
    ///     If the protocol operation of the RfcLdapMessage is of this type,
    ///     it contains at least an RfcLdapResult.
    /// </summary>
    public interface RfcResponse
    {
        /// <summary> </summary>
        Asn1Enumerated getResultCode();

        /// <summary> </summary>
        RfcLdapDN getMatchedDN();

        /// <summary> </summary>
        RfcLdapString getErrorMessage();

        /// <summary> </summary>
        Asn1SequenceOf getReferral();
    }

    /// <summary>
    ///     This interface represents Protocol Operations that are requests from a
    ///     client.
    /// </summary>
    public interface RfcRequest
    {
        /// <summary> Builds a new request using the data from the this object.</summary>
        RfcRequest dupRequest(string base_Renamed, string filter, bool reference);

        /// <summary> Builds a new request using the data from the this object.</summary>
        string getRequestDN();
    }

    /// <summary>
    ///     Represents an Ldap Message.
    ///     <pre>
    ///         LdapMessage ::= SEQUENCE {
    ///         messageID       MessageID,
    ///         protocolOp      CHOICE {
    ///         bindRequest     BindRequest,
    ///         bindResponse    BindResponse,
    ///         unbindRequest   UnbindRequest,
    ///         searchRequest   SearchRequest,
    ///         searchResEntry  SearchResultEntry,
    ///         searchResDone   SearchResultDone,
    ///         searchResRef    SearchResultReference,
    ///         modifyRequest   ModifyRequest,
    ///         modifyResponse  ModifyResponse,
    ///         addRequest      AddRequest,
    ///         addResponse     AddResponse,
    ///         delRequest      DelRequest,
    ///         delResponse     DelResponse,
    ///         modDNRequest    ModifyDNRequest,
    ///         modDNResponse   ModifyDNResponse,
    ///         compareRequest  CompareRequest,
    ///         compareResponse CompareResponse,
    ///         abandonRequest  AbandonRequest,
    ///         extendedReq     ExtendedRequest,
    ///         extendedResp    ExtendedResponse },
    ///         controls       [0] Controls OPTIONAL }
    ///     </pre>
    ///     Note: The creation of a MessageID should be hidden within the creation of
    ///     an RfcLdapMessage. The MessageID needs to be in sequence, and has an
    ///     upper and lower limit. There is never a case when a user should be
    ///     able to specify the MessageID for an RfcLdapMessage. The MessageID()
    ///     constructor should be package protected. (So the MessageID value
    ///     isn't arbitrarily run up.)
    /// </summary>
    public class RfcLdapMessage : Asn1Sequence
    {
        /// <summary> Returns this RfcLdapMessage's messageID as an int.</summary>
        public virtual int MessageID
        {
            get { return ((Asn1Integer)Get(0)).IntValue(); }
        }

        /// <summary> Returns this RfcLdapMessage's message type</summary>
        public virtual int Type
        {
            get { return Get(1).GetIdentifier().Tag; }
        }

        /// <summary>
        ///     Returns the response associated with this RfcLdapMessage.
        ///     Can be one of RfcLdapResult, RfcBindResponse, RfcExtendedResponse
        ///     all which extend RfcResponse. It can also be
        ///     RfcSearchResultEntry, or RfcSearchResultReference
        /// </summary>
        public virtual Asn1Object Response
        {
            get { return Get(1); }
        }

        /// <summary> Returns the optional Controls for this RfcLdapMessage.</summary>
        public virtual RfcControls Controls
        {
            get
            {
                if (Size() > 2)
                    return (RfcControls)Get(2);
                return null;
            }
        }

        /// <summary> Returns the dn of the request, may be null</summary>
        public virtual string RequestDN
        {
            get { return ((RfcRequest)op).getRequestDN(); }
        }

        /// <summary>
        ///     returns the original request in this message
        /// </summary>
        /// <returns>
        ///     the original msg request for this response
        /// </returns>
        /// <summary>
        ///     sets the original request in this message
        /// </summary>
        /// <param name="msg">
        ///     the original request for this response
        /// </param>
        public virtual LdapMessage RequestingMessage
        {
            get { return requestMessage; }

            set { requestMessage = value; }
        }

        private readonly Asn1Object op;
        private RfcControls controls;
        private LdapMessage requestMessage;

        /// <summary>
        ///     Create an RfcLdapMessage by copying the content array
        /// </summary>
        /// <param name="origContent">
        ///     the array list to copy
        /// </param>
        internal RfcLdapMessage(Asn1Object[] origContent, RfcRequest origRequest, string dn, string filter,
            bool reference) : base(origContent, origContent.Length)
        {
            Set(0, new RfcMessageID()); // MessageID has static counter

            var req = (RfcRequest)origContent[1];
            var newreq = req.dupRequest(dn, filter, reference);
            op = (Asn1Object)newreq;
            Set(1, (Asn1Object)newreq);
        }

        /// <summary> Create an RfcLdapMessage using the specified Ldap Request.</summary>
        public RfcLdapMessage(RfcRequest op) : this(op, null)
        {
        }

        /// <summary> Create an RfcLdapMessage request from input parameters.</summary>
        public RfcLdapMessage(RfcRequest op, RfcControls controls) : base(3)
        {
            this.op = (Asn1Object)op;
            this.controls = controls;

            Add(new RfcMessageID()); // MessageID has static counter
            Add((Asn1Object)op);
            if (controls != null)
            {
                Add(controls);
            }
        }

        /// <summary> Create an RfcLdapMessage using the specified Ldap Response.</summary>
        public RfcLdapMessage(Asn1Sequence op) : this(op, null)
        {
        }

        /// <summary> Create an RfcLdapMessage response from input parameters.</summary>
        public RfcLdapMessage(Asn1Sequence op, RfcControls controls) : base(3)
        {
            this.op = op;
            this.controls = controls;

            Add(new RfcMessageID()); // MessageID has static counter
            Add(op);
            if (controls != null)
            {
                Add(controls);
            }
        }

        /// <summary> Will decode an RfcLdapMessage directly from an InputStream.</summary>
        public RfcLdapMessage(Asn1Decoder dec, Stream in_Renamed, int len) : base(dec, in_Renamed, len)
        {
            sbyte[] content;
            MemoryStream bais;

            // Decode implicitly tagged protocol operation from an Asn1Tagged type
            // to its appropriate application type.
            var protocolOp = (Asn1Tagged)Get(1);
            var protocolOpId = protocolOp.GetIdentifier();
            content = ((Asn1OctetString)protocolOp.taggedValue()).ByteValue();
            bais = new MemoryStream(content.ToByteArray());

            switch (protocolOpId.Tag)
            {
                case LdapMessage.SEARCH_RESPONSE:
                    Set(1, new RfcSearchResultEntry(dec, bais, content.Length));
                    break;

                case LdapMessage.SEARCH_RESULT:
                    Set(1, new RfcSearchResultDone(dec, bais, content.Length));
                    break;

                case LdapMessage.SEARCH_RESULT_REFERENCE:
                    Set(1, new RfcSearchResultReference(dec, bais, content.Length));
                    break;
                    
                case LdapMessage.BIND_RESPONSE:
                    Set(1, new RfcBindResponse(dec, bais, content.Length));
                    break;
                    
                case LdapMessage.EXTENDED_RESPONSE:
                    Set(1, new RfcExtendedResponse(dec, bais, content.Length));
                    break;

                case LdapMessage.INTERMEDIATE_RESPONSE:
                    Set(1, new RfcIntermediateResponse(dec, bais, content.Length));
                    break;
                    
                default:
                    throw new Exception("RfcLdapMessage: Invalid tag: " + protocolOpId.Tag);
            }

            // decode optional implicitly tagged controls from Asn1Tagged type to
            // to RFC 2251 types.
            if (Size() > 2)
            {
                var controls = (Asn1Tagged)Get(2);
                //   Asn1Identifier controlsId = protocolOp.getIdentifier();
                // we could check to make sure we have controls here....

                content = ((Asn1OctetString)controls.taggedValue()).ByteValue();
                bais = new MemoryStream(content.ToByteArray());
                Set(2, new RfcControls(dec, bais, content.Length));
            }
        }

        //*************************************************************************
        // Accessors
        //*************************************************************************

        /// <summary>
        ///     Returns the request associated with this RfcLdapMessage.
        ///     Throws a class cast exception if the RfcLdapMessage is not a request.
        /// </summary>
        public RfcRequest getRequest()
        {
            return (RfcRequest)Get(1);
        }

        public virtual bool isRequest()
        {
            return Get(1) is RfcRequest;
        }

        /// <summary>
        ///     Duplicate this message, replacing base dn, filter, and scope if supplied
        /// </summary>
        /// <param name="dn">
        ///     the base dn
        /// </param>
        /// <param name="filter">
        ///     the filter
        /// </param>
        /// <param name="reference">
        ///     true if a search reference
        /// </param>
        /// <returns>
        ///     the object representing the new message
        /// </returns>
        public object dupMessage(string dn, string filter, bool reference)
        {
            if (op == null)
            {
                throw new LdapException("DUP_ERROR", LdapException.LOCAL_ERROR, null);
            }

            var newMsg = new RfcLdapMessage(ToArray(), (RfcRequest)Get(1), dn, filter, reference);
            return newMsg;
        }
    }

    /// <summary>
    ///     The base class for Ldap request and response messages.
    ///     Subclassed by response messages used in asynchronous operations.
    /// </summary>
    public class LdapMessage
    {
        /// <summary> Returns the LdapMessage request associated with this response</summary>
        internal virtual LdapMessage RequestingMessage
        {
            get { return message.RequestingMessage; }
        }

        /// <summary> Returns any controls in the message.</summary>
        public virtual LdapControl[] Controls
        {
            get
            {
                /*				LdapControl[] controls = null;
                                RfcControls asn1Ctrls = message.Controls;

                                if (asn1Ctrls != null)
                                {
                                    controls = new LdapControl[asn1Ctrls.size()];
                                    for (int i = 0; i < asn1Ctrls.size(); i++)
                                    {
                                        RfcControl rfcCtl = (RfcControl) asn1Ctrls.Get(i);
                                        System.String oid = rfcCtl.ControlType.stringValue();
                                        sbyte[] value_Renamed = rfcCtl.ControlValue.byteValue();
                                        bool critical = rfcCtl.Criticality.booleanValue();

                                        controls[i] = controlFactory(oid, critical, value_Renamed);
                                    }
                                }

                                return controls;
                */
                LdapControl[] controls = null;
                var asn1Ctrls = message.Controls;

                // convert from RFC 2251 Controls to LDAPControl[].
                if (asn1Ctrls != null)
                {
                    controls = new LdapControl[asn1Ctrls.Size()];
                    for (var i = 0; i < asn1Ctrls.Size(); i++)
                    {
                        /*
                                                * At this point we have an RfcControl which needs to be
                                                * converted to the appropriate Response Control.  This requires
                                                * calling the constructor of a class that extends LDAPControl.
                                                * The controlFactory method searches the list of registered
                                                * controls and if a match is found calls the constructor
                                                * for that child LDAPControl. Otherwise, it returns a regular
                                                * LDAPControl object.
                                                *
                                                * Question: Why did we not call the controlFactory method when
                                                * we were parsing the control. Answer: By the time the
                                                * code realizes that we have a control it is already too late.
                                                */
                        var rfcCtl = (RfcControl)asn1Ctrls.Get(i);
                        var oid = rfcCtl.ControlType.StringValue();
                        var value_Renamed = rfcCtl.ControlValue.ByteValue();
                        var critical = rfcCtl.Criticality.BooleanValue();

                        /* Return from this call should return either an LDAPControl
                        * or a class extending LDAPControl that implements the
                        * appropriate registered response control
                        */
                        controls[i] = controlFactory(oid, critical, value_Renamed);
                    }
                }
                return controls;
            }
        }

        /// <summary>
        ///     Returns the message ID.  The message ID is an integer value
        ///     identifying the Ldap request and its response.
        /// </summary>
        public virtual int MessageID
        {
            get
            {
                if (imsgNum == -1)
                {
                    imsgNum = message.MessageID;
                }
                return imsgNum;
            }
        }

        /// <summary>
        ///     Returns the Ldap operation type of the message.
        ///     The type is one of the following:
        ///     <ul>
        ///         <li>BIND_REQUEST            = 0;</li>
        ///         <li>BIND_RESPONSE           = 1;</li>
        ///         <li>UNBIND_REQUEST          = 2;</li>
        ///         <li>SEARCH_REQUEST          = 3;</li>
        ///         <li>SEARCH_RESPONSE         = 4;</li>
        ///         <li>SEARCH_RESULT           = 5;</li>
        ///         <li>MODIFY_REQUEST          = 6;</li>
        ///         <li>MODIFY_RESPONSE         = 7;</li>
        ///         <li>ADD_REQUEST             = 8;</li>
        ///         <li>ADD_RESPONSE            = 9;</li>
        ///         <li>DEL_REQUEST             = 10;</li>
        ///         <li>DEL_RESPONSE            = 11;</li>
        ///         <li>MODIFY_RDN_REQUEST      = 12;</li>
        ///         <li>MODIFY_RDN_RESPONSE     = 13;</li>
        ///         <li>COMPARE_REQUEST         = 14;</li>
        ///         <li>COMPARE_RESPONSE        = 15;</li>
        ///         <li>ABANDON_REQUEST         = 16;</li>
        ///         <li>SEARCH_RESULT_REFERENCE = 19;</li>
        ///         <li>EXTENDED_REQUEST        = 23;</li>
        ///         <li>EXTENDED_RESPONSE       = 24;</li>
        ///         <li>INTERMEDIATE_RESPONSE   = 25;</li>
        ///     </ul>
        /// </summary>
        /// <returns>
        ///     The operation type of the message.
        /// </returns>
        public virtual int Type
        {
            get
            {
                if (messageType == -1)
                {
                    messageType = message.Type;
                }
                return messageType;
            }
        }

        /// <summary>
        ///     Indicates whether the message is a request or a response
        /// </summary>
        /// <returns>
        ///     true if the message is a request, false if it is a response,
        ///     a search result, or a search result reference.
        /// </returns>
        public virtual bool Request
        {
            get { return message.isRequest(); }
        }

        /// <summary> Returns the RFC 2251 LdapMessage composed in this object.</summary>
        internal virtual RfcLdapMessage Asn1Object
        {
            get { return message; }
        }

        private string Name
        {
            get
            {
                string name;
                switch (Type)
                {
                    case SEARCH_RESPONSE:
                        name = "LdapSearchResponse";
                        break;

                    case SEARCH_RESULT:
                        name = "LdapSearchResult";
                        break;

                    case SEARCH_REQUEST:
                        name = "LdapSearchRequest";
                        break;
                        
                    case BIND_REQUEST:
                        name = "LdapBindRequest";
                        break;

                    case BIND_RESPONSE:
                        name = "LdapBindResponse";
                        break;

                    case UNBIND_REQUEST:
                        name = "LdapUnbindRequest";
                        break;

                    case ABANDON_REQUEST:
                        name = "LdapAbandonRequest";
                        break;

                    case SEARCH_RESULT_REFERENCE:
                        name = "LdapSearchResultReference";
                        break;

                    case EXTENDED_REQUEST:
                        name = "LdapExtendedRequest";
                        break;

                    case EXTENDED_RESPONSE:
                        name = "LdapExtendedResponse";
                        break;

                    case INTERMEDIATE_RESPONSE:
                        name = "LdapIntermediateResponse";
                        break;

                    default:
                        throw new Exception("LdapMessage: Unknown Type " + Type);
                }
                return name;
            }
        }

        /// <summary>
        ///     Retrieves the identifier tag for this message.
        ///     An identifier can be associated with a message with the
        ///     <code>setTag</code> method.
        ///     Tags are set by the application and not by the API or the server.
        ///     If a server response <code>isRequest() == false</code> has no tag,
        ///     the tag associated with the corresponding server request is used.
        /// </summary>
        /// <returns>
        ///     the identifier associated with this message or <code>null</code>
        ///     if none.
        /// </returns>
        /// <summary>
        ///     Sets a string identifier tag for this message.
        ///     This method allows an API to set a tag and later identify messages
        ///     by retrieving the tag associated with the message.
        ///     Tags are set by the application and not by the API or the server.
        ///     Message tags are not included with any message sent to or received
        ///     from the server.
        ///     Tags set on a request to the server
        ///     are automatically associated with the response messages when they are
        ///     received by the API and transferred to the application.
        ///     The application can explicitly set a different value in a
        ///     response message.
        ///     To set a value in a server request, for example an
        ///     {@link LdapSearchRequest}, you must create the object,
        ///     set the tag, and use the
        ///     {@link LdapConnection.SendRequest LdapConnection.sendRequest()}
        ///     method to send it to the server.
        /// </summary>
        /// <param name="stringTag">
        ///     the String assigned to identify this message.
        /// </param>
        public virtual string Tag
        {
            get
            {
                if ((object)stringTag != null)
                {
                    return stringTag;
                }
                if (Request)
                {
                    return null;
                }
                var m = RequestingMessage;
                if (m == null)
                {
                    return null;
                }
                return m.stringTag;
            }

            set { stringTag = value; }
        }

        /// <summary>
        ///     A bind request operation.
        ///     BIND_REQUEST = 0
        /// </summary>
        public const int BIND_REQUEST = 0;

        /// <summary>
        ///     A bind response operation.
        ///     BIND_RESPONSE = 1
        /// </summary>
        public const int BIND_RESPONSE = 1;

        /// <summary>
        ///     An unbind request operation.
        ///     UNBIND_REQUEST = 2
        /// </summary>
        public const int UNBIND_REQUEST = 2;

        /// <summary>
        ///     A search request operation.
        ///     SEARCH_REQUEST = 3
        /// </summary>
        public const int SEARCH_REQUEST = 3;

        /// <summary>
        ///     A search response containing data.
        ///     SEARCH_RESPONSE = 4
        /// </summary>
        public const int SEARCH_RESPONSE = 4;

        /// <summary>
        ///     A search result message - contains search status.
        ///     SEARCH_RESULT = 5
        /// </summary>
        public const int SEARCH_RESULT = 5;
        
        /// <summary>
        ///     An abandon request operation.
        ///     ABANDON_REQUEST = 16
        /// </summary>
        public const int ABANDON_REQUEST = 16;

        /// <summary>
        ///     A search result reference operation.
        ///     SEARCH_RESULT_REFERENCE = 19
        /// </summary>
        public const int SEARCH_RESULT_REFERENCE = 19;

        /// <summary>
        ///     An extended request operation.
        ///     EXTENDED_REQUEST = 23
        /// </summary>
        public const int EXTENDED_REQUEST = 23;

        /// <summary>
        ///     An extended response operation.
        ///     EXTENDED_RESONSE = 24
        /// </summary>
        public const int EXTENDED_RESPONSE = 24;

        /// <summary>
        ///     An intermediate response operation.
        ///     INTERMEDIATE_RESONSE = 25
        /// </summary>
        public const int INTERMEDIATE_RESPONSE = 25;

        /// <summary> A request or response message for an asynchronous Ldap operation.</summary>
        protected internal RfcLdapMessage message;

        /// <summary> Lock object to protect counter for message numbers</summary>
        /// <summary>
        ///     Counters used to construct request message #'s, unique for each request
        ///     Will be enabled after ASN.1 conversion
        /// </summary>
        /*
        private static int msgNum = 0; // Ldap Request counter
        */
        private int imsgNum = -1; // This instance LdapMessage number

        private int messageType = -1;

        /* application defined tag to identify this message */
        private string stringTag;

        /// <summary> Dummy constuctor</summary>
        internal LdapMessage()
        {
        }

        /// <summary>
        ///     Creates an LdapMessage when sending a protocol operation and sends
        ///     some optional controls with the message.
        /// </summary>
        /// <param name="op">
        ///     The operation type of message.
        /// </param>
        /// <param name="controls">
        ///     The controls to use with the operation.
        /// </param>
        /// <seealso cref="Type">
        /// </seealso>
        /*package*/
        internal LdapMessage(int type, RfcRequest op, LdapControl[] controls)
        {
            // Get a unique number for this request message

            messageType = type;
            RfcControls asn1Ctrls = null;
            if (controls != null)
            {
                // Move LdapControls into an RFC 2251 Controls object.
                asn1Ctrls = new RfcControls();
                for (var i = 0; i < controls.Length; i++)
                {
                    //					asn1Ctrls.add(null);
                    asn1Ctrls.add(controls[i].Asn1Object);
                }
            }

            // create RFC 2251 LdapMessage
            message = new RfcLdapMessage(op, asn1Ctrls);
        }

        /// <summary>
        ///     Creates an Rfc 2251 LdapMessage when the libraries receive a response
        ///     from a command.
        /// </summary>
        /// <param name="message">
        ///     A response message.
        /// </param>
        protected internal LdapMessage(RfcLdapMessage message)
        {
            this.message = message;
        }

        /// <summary>
        ///     Returns a mutated clone of this LdapMessage,
        ///     replacing base dn, filter.
        /// </summary>
        /// <param name="dn">
        ///     the base dn
        /// </param>
        /// <param name="filter">
        ///     the filter
        /// </param>
        /// <param name="reference">
        ///     true if a search reference
        /// </param>
        /// <returns>
        ///     the object representing the new message
        /// </returns>
        internal LdapMessage Clone(string dn, string filter, bool reference)
        {
            return new LdapMessage((RfcLdapMessage)message.dupMessage(dn, filter, reference));
        }

        /// <summary>
        ///     Instantiates an LdapControl.  We search through our list of
        ///     registered controls.  If we find a matchiing OID we instantiate
        ///     that control by calling its contructor.  Otherwise we default to
        ///     returning a regular LdapControl object
        /// </summary>
        private LdapControl controlFactory(string oid, bool critical, sbyte[] value_Renamed)
        {
            var regControls = LdapControl.RegisteredControls;
            try
            {
                /*
                * search through the registered extension list to find the
                * response control class
                */
                var respCtlClass = regControls.findResponseControl(oid);

                // Did not find a match so return default LDAPControl
                if (respCtlClass == null)
                    return new LdapControl(oid, critical, value_Renamed);

                /* If found, get LDAPControl constructor */
                Type[] argsClass = { typeof(string), typeof(bool), typeof(sbyte[]) };
                object[] args = { oid, critical, value_Renamed };
                try
                {
                    var ctlConstructor = respCtlClass.GetConstructor(argsClass);

                    try
                    {
                        /* Call the control constructor for a registered Class*/
                        object ctl = null;
                        //						ctl = ctlConstructor.newInstance(args);
                        ctl = ctlConstructor.Invoke(args);
                        return (LdapControl)ctl;
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (TargetInvocationException e)
                    {
                    }
                    catch (Exception e)
                    {
                        // Could not create the ResponseControl object
                        // All possible exceptions are ignored. We fall through
                        // and create a default LDAPControl object
                    }
                }
                catch (MethodAccessException e)
                {
                    // bad class was specified, fall through and return a
                    // default LDAPControl object
                }
            }
            catch (FieldAccessException)
            {
                // No match with the OID
                // Do nothing. Fall through and construct a default LDAPControl object.
            }
            // If we get here we did not have a registered response control
            // for this oid.  Return a default LDAPControl object.
            return new LdapControl(oid, critical, value_Renamed);
        }

        /// <summary>
        ///     Creates a String representation of this object
        /// </summary>
        /// <returns>
        ///     a String representation for this LdapMessage
        /// </returns>
        public override string ToString()
        {
            return Name + "(" + MessageID + "): " + message;
        }
    }

    /// <summary>
    ///     Encapsulates the response returned by an Ldap server on an
    ///     asynchronous extended operation request.  It extends LdapResponse.
    ///     The response can contain the OID of the extension, an octet string
    ///     with the operation's data, both, or neither.
    /// </summary>
    public class LdapExtendedResponse : LdapResponse
    {
        /// <summary>
        ///     Returns the message identifier of the response.
        /// </summary>
        /// <returns>
        ///     OID of the response.
        /// </returns>
        public virtual string ID
        {
            get
            {
                var respOID = ((RfcExtendedResponse)message.Response).ResponseName;
                return respOID?.StringValue();
            }
        }

        static LdapExtendedResponse()
        {
            registeredResponses = new RespExtensionSet();
        }

        public static RespExtensionSet RegisteredResponses => registeredResponses;

        /// <summary>
        ///     Returns the value part of the response in raw bytes.
        /// </summary>
        /// <returns>
        ///     The value of the response.
        /// </returns>
        public virtual sbyte[] Value
        {
            get
            {
                var tempString = ((RfcExtendedResponse)message.Response).Response;
                return tempString?.ByteValue();
            }
        }

        private static readonly RespExtensionSet registeredResponses;

        /// <summary>
        ///     Creates an LdapExtendedResponse object which encapsulates
        ///     a server response to an asynchronous extended operation request.
        /// </summary>
        /// <param name="message">
        ///     The RfcLdapMessage to convert to an
        ///     LdapExtendedResponse object.
        /// </param>
        public LdapExtendedResponse(RfcLdapMessage message) : base(message)
        {
        }

        /// <summary>
        ///     Registers a class to be instantiated on receipt of a extendedresponse
        ///     with the given OID.
        ///     <p>
        ///         Any previous registration for the OID is overridden. The
        ///         extendedResponseClass object MUST be an extension of
        ///         LDAPExtendedResponse.
        ///     </p>
        /// </summary>
        /// <param name="oid">
        ///     The object identifier of the control.
        /// </param>
        /// <param name="extendedResponseClass">
        ///     A class which can instantiate an
        ///     LDAPExtendedResponse.
        /// </param>
        public static void register(string oid, Type extendedResponseClass)
        {
            registeredResponses.registerResponseExtension(oid, extendedResponseClass);
        }
    }
}
#endif