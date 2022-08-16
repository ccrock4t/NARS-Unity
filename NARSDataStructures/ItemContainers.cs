using System.Collections;
using System.Collections.Generic;

/*
    Author: Christian Hahm
    Created: May 19, 2022
*/
public abstract class ItemContainer<T> : IEnumerable<Item<T>>
{
    /*
        Base Class for data structures which contain "Items", as defined in this class.

        Examples of Item Containers include Bag and Buffer.
    */

    public Dictionary<string, Item<T>> item_lookup_dict;
    int next_item_id;
    public int capacity;

    public Item<T> this[string key]
    {
        get => this.item_lookup_dict[key];
    }


    public ItemContainer(int capacity)
    {
        this.item_lookup_dict = new Dictionary<string, Item<T>>();  // for accessing Item by key
        this.next_item_id = 0;
        this.capacity = capacity;
    }

    public bool Contains(T obj)
    {
        /*
            Purpose:
                Check if the object is contained in the Bag by checking whether its key is in the item lookup table

        :param object: object to look for in the Bag
        :return: true if the item is in the Bag;
                    false otherwise
        */
        string key = Item<T>.get_key_from_object(obj);
        return this.item_lookup_dict.ContainsKey(key);
    }

    public IEnumerator<Item<T>> GetEnumerator()
    {
        foreach (KeyValuePair<string, Item<T>> item in this.item_lookup_dict)
        {
            yield return item.Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<Item<T>>)this.item_lookup_dict).GetEnumerator();
    }



    public void Clear()
    {
        this.item_lookup_dict.Clear();
        this.next_item_id = 0;
    }

    public virtual Item<T> PUT_NEW(T obj)
    {
        /*
            Place a NEW Item into the container.
        */
        Item<T> item = new Item<T>(obj, this.get_next_item_id());
        this._put_into_lookup_dict(item);  // Item Container
        return item;
    }

    public Item<T> peek_using_key(string key)
    {
        /*
            Peek an Item using its key

            :param key: Key of the item to peek
            :return: Item peeked from the data structure
        */
        return this.item_lookup_dict[key];
    }

    public void _put_into_lookup_dict(Item<T> item)
    {
        /*
        Puts item into lookup table && GUI
        :param item: put an Item into the lookup dictionary.
        */
        // put item into lookup table
        this.item_lookup_dict.Add(item.key, item);
    }

    public Item<T> _take_from_lookup_dict(string key)
    {
        /*
                Removes an Item from the lookup dictionary using its key,
                && returns the Item.

            :param key: Key of the Item to remove.
            :return: The Item that was removed.
            */
        Item<T> item = this.item_lookup_dict[key];
        this.item_lookup_dict.Remove(key);  // remove item reference from lookup table
        return item;
    }


    public int get_next_item_id()
    {
        this.next_item_id++;
        return this.next_item_id - 1;
    }


    public int GetCount()
    {
        return item_lookup_dict.Count;
    }
}

public class Item<T>
{
    /*
        Item in an Item Container. Wraps an object.

        Consists of:
            object (e.g. Concept, Task, etc.)

            budget ($priority$)
    */

    public int? bucket_num;
    public int? quality_bucket_num;
    public string key;
    public int id;
    public T obj;
    public Budget budget;

    public Item(T obj, int id, float priority=0.99f, float quality=0.01f)
    {
        /*
        :param object: object to wrap in the item
        :param container: the Item Container instance that will contain this item
        */
        this.bucket_num = null;
        this.obj = obj;
        this.id = id;

        // assign ID
        this.key = Item<T>.get_key_from_object(obj);
        this.budget = new Budget(priority, quality);
    }


    public static string get_key_from_object(T obj)
    {
        /*
        Returns a key that uniquely identifies the given object.

        This == essentially a universal hash function for NARS objects

        :param object:
        :return: key for object
    */
        string key = null;
        if (obj is Concept)
        {
            key = (obj as Concept).term.ToString();
        }
        else if (obj is Sentence)
        {
            key = (obj as Sentence).stamp.id.ToString();
        }
        else
        {
            key = obj.ToString();
        }
        return key;
    }

    public string ToString()
    {
        return SyntaxUtils.stringValueOf(StatementSyntax.BudgetMarker)
        + this.budget.get_priority()
       + SyntaxUtils.stringValueOf(StatementSyntax.ValueSeparator)
       + this.budget.get_quality()
       + SyntaxUtils.stringValueOf(StatementSyntax.BudgetMarker)
       + " "
        + NALSyntax.MARKER_ITEM_ID
        + this.id + NALSyntax.MARKER_ID_END
        + this.obj.ToString();
    }



    /*    def get_gui_info(this)
                        {
                            dict = { }
                            dict[NARSGUI.NARSGUI.KEY_KEY] = this.key
            dict[NARSGUI.NARSGUI.KEY_CLASS_NAME] = type(this.object).__name__
            dict[NARSGUI.NARSGUI.KEY_OBJECT_STRING] = str(this.object)
            dict[NARSGUI.NARSGUI.KEY_TERM_TYPE] = type(this.object.get_term()).__name__
            if isinstance(this.object, NARSMemory.Concept){
                                dict[NARSGUI.NARSGUI.KEY_IS_POSITIVE] = "true" if this.object.is_positive() else "false"
                if len(this.object.desire_table) > 0:
                    dict[NARSGUI.NARSGUI.KEY_PASSES_DECISION] = "true" if Local.Decision(
                        this.object.desire_table.peek()) else "false"
                else:
                    dict[NARSGUI.NARSGUI.KEY_PASSES_DECISION] = null
                dict[NARSGUI.NARSGUI.KEY_EXPECTATION] = this.object.get_expectation()
                dict[NARSGUI.NARSGUI.KEY_LIST_BELIEFS] = [str(belief[0]) for belief in this.object.belief_table]
                dict[NARSGUI.NARSGUI.KEY_LIST_DESIRES] = [str(desire[0]) for desire in this.object.desire_table]
                dict[NARSGUI.NARSGUI.KEY_LIST_TERM_LINKS] = [str(termlink.object) for termlink in this.object.term_links]
                dict[NARSGUI.NARSGUI.KEY_LIST_PREDICTION_LINKS] = [str(predictionlink.object) for predictionlink in
                                                                   this.object.prediction_links]
                dict[NARSGUI.NARSGUI.KEY_LIST_EXPLANATION_LINKS] = [str(explanationlink.object) for explanationlink in
                                                                    this.object.explanation_links]
                dict[NARSGUI.NARSGUI.KEY_CAPACITY_BELIEFS] = str(this.object.belief_table.capacity)
                dict[NARSGUI.NARSGUI.KEY_CAPACITY_DESIRES] = str(this.object.desire_table.capacity)
                dict[NARSGUI.NARSGUI.KEY_CAPACITY_TERM_LINKS] = str(this.object.term_links.capacity)
                dict[NARSGUI.NARSGUI.KEY_CAPACITY_PREDICTION_LINKS] = str(this.object.prediction_links.capacity)
                dict[NARSGUI.NARSGUI.KEY_CAPACITY_EXPLANATION_LINKS] = str(this.object.explanation_links.capacity)
            else if isinstance(this.object, NARSDataStructures.Other.Task){
                                                dict[NARSGUI.NARSGUI.KEY_SENTENCE_STRING] = str(this.object.sentence)
                dict[NARSGUI.NARSGUI.KEY_LIST_EVIDENTIAL_BASE] = [str(evidence) for evidence in
                                                                  this.object.sentence.stamp.evidential_base]
                dict[NARSGUI.NARSGUI.KEY_LIST_INTERACTED_SENTENCES] = []

            return dict*/
}

public class Budget
{
    /*
        Budget deciding the proportion of the system's time-space resources to allocate to a Bag Item.
        Priority determines how likely an item is to be selected,
        Quality defines the Item's base priority (its lowest possible priority)
    */

    float _priority;
    float _quality;

    public Budget(float priority, float quality)
    {
        this.set_quality(quality);
        this.set_priority(priority);
    }

    public override string ToString()
    {
        return SyntaxUtils.stringValueOf(StatementSyntax.BudgetMarker)
                   + this.get_priority()
                   + SyntaxUtils.stringValueOf(StatementSyntax.ValueSeparator)
                   + this.get_quality()
                   + SyntaxUtils.stringValueOf(StatementSyntax.BudgetMarker);
    }

    public void set_priority(float value)
    {
        // if value < this.get_quality(){ value = this.get_quality()  // priority can't go below quality
        if(value > 0.99999999f) value = 0.99999999f;  // priority can't got too close to 1
        if(value < 0) value = 0;  // priority can't go below 0
        this._priority = value;
    }


    public void set_quality(float value)
    {
        if(value > 0.99999999) value = 0.99999999f;  // quality can't got too close to 1
        if(value < 0) value = 0; // priority can't go below 0
        this._quality = value;
    }


    public float get_priority()
    {
        return this._priority;
    }

    public float get_quality()
    {
        return this._quality;
    }
}