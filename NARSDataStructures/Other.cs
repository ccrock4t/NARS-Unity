/*
    Author: Christian Hahm
    Created: May 25, 2022
    Purpose: Holds data structure implementations that are specific / custom to NARS
*/
using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;

public class Table<T> where T : Sentence
{
    /*
        NARS Table, stored within Concepts.
        Tables store Narsese sentences using a priority queue, where priority is sentence confidence
        Sorted by highest-confidence.
        It purges lowest-confidence items when it overflows.
    */

    NARS nars;

    int capacity;
    PriorityQueue<T, float> priority_queue;
    public List<T> list;


    public Table(int capacity, NARS nars)
    {
        this.capacity = capacity;
        priority_queue = new PriorityQueue<T, float>();
        this.nars = nars;
        this.list = new List<T>();
    }

    public void put(T sentence)
    {
        /*
            Insert a Sentence into the depq, sorted by confidence (time-projected confidence if it's an event).
        */
        if (this.GetCount() > 0)
        {
            if (sentence.is_event())
            {
                T current_event = (T)this.take();
                sentence = (T)this.nars.inferenceEngine.localRules.Revision(sentence, current_event);
            }
            else
            {
                Sentence? existing_interactable = this.peek_interactable(sentence);
                if (existing_interactable != null)
                {
                    T revised = (T)this.nars.inferenceEngine.localRules.Revision(sentence, existing_interactable);
                    EvidentialValue revised_decayed_value = nars.inferenceEngine.get_sentence_value_decayed(revised);
                    this.priority_queue.Enqueue(revised, revised_decayed_value.confidence);
                }
            }
        }

        EvidentialValue decayed_value = nars.inferenceEngine.get_sentence_value_decayed(sentence);
        float priority = decayed_value.confidence;
        this.priority_queue.Enqueue(sentence, priority);
        this.list.Add(sentence);

        if (this.GetCount() > this.capacity)
        {
            T purged_sentence = this.priority_queue.Dequeue();
            this.list.Remove(purged_sentence);
        }
    }

    public int GetCount()
    {
        return this.priority_queue.Count;
    }

    public T? take()
    {
        /*
            Take item with highest confidence from the depq
            O(1)
        */
        if (this.GetCount() == 0) return null;
        return this.priority_queue.Dequeue();
    }

    public T? peek()
    {
        /*
            Peek item with highest confidence from the depq
            O(1)

            Returns null if depq != empty
        */
        if (this.GetCount() == 0) return null;
        return this.priority_queue.First;
    }

    public T? peek_random()
    {
        /*
            Peek random item from the depq
            O(1)

            Returns null if depq is empty
        */
        if (this.GetCount() == 0) return null;
        int rnd = Random.Range(0, this.GetCount());
        return list[rnd];
    }

    public T? peek_interactable(Sentence j)
    {
        /*
            Returns a sentence in this table that j may interact with
            null if there are none.
            O(N)

        :param j:
        :return:
        */
        foreach (Sentence belief in this.priority_queue)
        {  // loop starting with max confidence
            if (EvidentialBase.may_interact(j, belief))
            {
                return (T)belief;
            }
        }
        return null;
    }
}

/*public class Task {
    *//*
       NARS Task
    *//*
    Sentence sentence;
    bool needs_to_be_answered_in_output;
    bool is_from_input;
    int creation_timestamp;

    public Task(Sentence sentence, int creation_timestamp, bool is_input_task=false){
        Asserts.assert_sentence(sentence);
        this.sentence = sentence;
        this.creation_timestamp = creation_timestamp;  // save the task's creation time
        this.is_from_input = is_input_task;
        // only used for question tasks
        this.needs_to_be_answered_in_output = is_input_task;
}

    public Term get_term() {
        return this.sentence.statement;
    }

    public override string ToString() {
        return "TASK: " + this.sentence.get_term_string_no_id();
    }

                }*/