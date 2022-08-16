/*
    Author: Christian Hahm
    Created: May 26, 2022
    Purpose: Defines NARS internal memory
*/


using UnityEngine;

public class Memory
{
    public NARS nars;
    /*
        NARS Memory
    */
    public Bag<Concept> concepts_bag;
    public int next_stamp_id = 0;
    public int next_percept_id = 0;

    public Memory(int capacity, NARS nars)
    {
        this.nars = nars;

        this.concepts_bag = new Bag<Concept>(capacity, nars.config.BAG_GRANULARITY);
    }


    public Concept get_random_concept()
    {
        /*
            Probabilistically peek the concepts
        */
        return this.concepts_bag.peek().obj;
    }

    public Item<Concept> get_random_concept_item()
    {
        /*
            Probabilistically peek the concepts
        */
        return this.concepts_bag.peek();
    }

    public int get_number_of_concepts()
    {
        /*
        Get the number of concepts that exist in memory
        */
        return this.concepts_bag.GetCount();
    }

    public Item<Concept> conceptualize_term(Term term)
    {
        /*
        Create a new concept from a term and add it to the bag

        :param term: The term naming the concept to create
        :returns New Concept item created from the term
        */
        Asserts.assert_term(term);
        string concept_key = Item<Term>.get_key_from_object(term);
        Asserts.assert(!this.concepts_bag.item_lookup_dict.ContainsKey(concept_key), "Cannot create new concept. Concept already exists.");
        // create new concept
        Concept new_concept = new Concept(term, this.nars);

        // put into data structure
        this.concepts_bag.PUT_NEW(new_concept); // add to bag

        if (term is CompoundTerm)
        {
            //todo allow array elements
            for (int i = 0; i < ((CompoundTerm)term).subterms.Count; i++)
            {
                Term subterm = ((CompoundTerm)term).subterms[i];
                // get/create subterm concepts
                if (!(subterm is VariableTerm))
                {  // don't create concepts for variables || array elements
                    Concept subconcept = this.peek_concept(subterm);
                    // do term linking with subterms
                    new_concept.set_term_links(subconcept);
                }
            }

        }
        else if (term is StatementTerm)
        {
            Concept subject_concept = this.peek_concept(((StatementTerm)term).get_subject_term());
            Concept predicate_concept = this.peek_concept(((StatementTerm)term).get_predicate_term());

            new_concept.set_term_links(subject_concept);
            new_concept.set_term_links(predicate_concept);

            if (!((StatementTerm)term).is_first_order())
            {
                // implication statement
                // do prediction/explanation linking with subterms
                if (subject_concept != null) subject_concept.set_prediction_link(new_concept);
                if (predicate_concept != null) predicate_concept.set_explanation_link(new_concept);
            }
        }

        Item<Concept> concept_item = this.peek_concept_item(term);

        return concept_item;
    }

    public Concept peek_concept(Term term)
    {
        Item<Concept> item = this.peek_concept_item(term);
        if (item == null) return null;
        return item.obj;
    }

    public Item<Concept> peek_concept_item(Term term)
    {
        /*
          Peek the concept from memory using its term,
          AND create it if it doesn't exist.
          Also recursively creates all sub-term concepts if they do not exist.

          If it's an `open` variable term, the concept != created, though if it has sub-terms
           those concepts will be created.

          :param term: The term naming the concept to peek
          :return Concept item named by the term
      */
        if (term is VariableTerm) return null; //todo created concepts for closed variable terms

        // try to find the existing concept
        string concept_key = Item<Term>.get_key_from_object(term);

        Item<Concept>? concept_item = this.concepts_bag.peek(concept_key);

        if (concept_item != null) return concept_item;  // return if it already exists

        // if it doesn't exist
        // it must be created along with its sub-concepts if necessary
        concept_item = this.conceptualize_term(term);

        return concept_item;
    }


    public Concept get_semantically_related_concept(Concept statement_concept)
    {
        /*
        Get concepts (named by a Statement Term) that are semantically related to the given concept.

        Using term-links, returns a concept with the same copula order; one for the subject && one for the predicate.

        For a first-order statement, may try to instead return higher-order concepts based on implication links

        :param statement_concept - Statement-Term Concept for which to find a semantically related Statement-Term concept

        :return Statement-Term Concept semantically related to param: `statement_concept`
    */

        int count = 0;
        Concept? related_concept = null;
        if (statement_concept.term_links.GetCount() == 0) return null;

        StatementTerm concept_term = (StatementTerm)statement_concept.term;

        while (count < this.nars.config.NUMBER_OF_ATTEMPTS_TO_SEARCH_FOR_SEMANTICALLY_RELATED_CONCEPT && related_concept == null)
        {
            count += 1;
            Concept shared_term_concept = statement_concept.term_links.peek().obj;
            if (concept_term.is_first_order())
            {
                // S --> P
                if (statement_concept.term_links.GetCount() != 0)
                    shared_term_concept = statement_concept.term_links.peek().obj;
                if (shared_term_concept.term is AtomicTerm)
                {
                    // atomic term concept (S)
                    related_concept = shared_term_concept.term_links.peek().obj; // peek additional term links to get another statement term
                }
                else if (shared_term_concept.term is CompoundTerm)
                {
                    if (((CompoundTerm)shared_term_concept.term).is_first_order())
                    {
                        // the subject || predicate == a first-order compound
                        related_concept = shared_term_concept.term_links.peek().obj; // peek additional term links to get a statement term
                        if (!(related_concept.term is StatementTerm))
                        {
                            related_concept = null;
                        }
                    }
                    else
                    {
                        // this statement == in a higher-order compound, we can use it in inference
                        related_concept = shared_term_concept;
                    }
                }
                else if (shared_term_concept.term is StatementTerm)
                {
                    // implication statement (S-->P) ==> B
                    related_concept = shared_term_concept;
                }
            }
            else
            {
                // Higher order
                // S ==> P
                // term linked concept == A-->B
                Bag<Concept> bag;
                if (shared_term_concept.prediction_links.GetCount() == 0 && shared_term_concept.explanation_links.GetCount() == 0)
                {
                    continue;
                }
                else if (shared_term_concept.prediction_links.GetCount() != 0 && shared_term_concept.explanation_links.GetCount() == 0)
                {
                    bag = shared_term_concept.prediction_links;
                }
                else if (shared_term_concept.explanation_links.GetCount() != 0 && shared_term_concept.prediction_links.GetCount() == 0)
                {
                    bag = shared_term_concept.explanation_links;
                }
                else
                {
                    int rand_int = Random.Range(0, 1);
                    switch (rand_int)
                    {
                        case 0:
                            bag = shared_term_concept.prediction_links;
                            break;
                        case 1:
                            bag = shared_term_concept.explanation_links;
                            break;
                        default:
                            Asserts.assert(false, "ERROR:");
                            continue;
                            break;
                    }

                    related_concept = bag.peek().obj;
                }
            }
        }

        return related_concept;
    }

    public Judgment get_best_explanation(Sentence j)
    {
        /*
            Gets the best explanation belief for the given sentence's statement
            that the sentence is able to interact with
            :param statement_concept:
            :return:
        */
        Concept statement_concept = this.peek_concept(j.statement); // B
        Judgment? best_explanation_belief = null;
        foreach (Item<Concept> explanation_concept_item in statement_concept.explanation_links)
        {

            Concept explanation_concept = explanation_concept_item.obj;  // A =/> B
            if (explanation_concept.belief_table.GetCount() == 0) continue;

            Judgment belief = (Judgment)explanation_concept.belief_table.peek_interactable(j);

            if (belief != null)
            {
                if (best_explanation_belief == null)
                {
                    best_explanation_belief = belief;
                }
                else
                {
                    best_explanation_belief = (Judgment)this.nars.inferenceEngine.localRules.Choice(belief, best_explanation_belief);
                }
            }
        }

        return best_explanation_belief;
    }

/*    public Judgment? get_explanation_preferred_with_true_precondition(Sentence j)
    {
        *//*
            Gets the best explanation belief for the given sentence's statement
            that the sentence is able to interact with
            :param statement_concept:
            :return:
        *//*
        Concept statement_concept = this.peek_concept(j.statement); // B
        if (statement_concept.explanation_links.GetCount() == 0) return null;
        Judgment? best_explanation_belief = null;
        int count = 0;
        int MAX_ATTEMPTS = this.nars.config.NUMBER_OF_ATTEMPTS_TO_SEARCH_FOR_SEMANTICALLY_RELATED_BELIEF;

        while (count < MAX_ATTEMPTS)
        {
            Item<Concept> item = statement_concept.explanation_links.peek();
            Concept explanation_concept = item.obj;  // A =/> B
            StatementTerm explanation_term = (StatementTerm)explanation_concept.term;
            Concept subject_concept = this.peek_concept(explanation_term.get_subject_term());
            if (subject_concept.contains_positive())
            {
                // (A &/ B) =/> C && A.
                Judgment? belief = (Judgment?)explanation_concept.belief_table.peek();
                if (belief != null)
                {
                    if (best_explanation_belief == null)
                    {
                        best_explanation_belief = belief;
                    }
                    else
                    {
                        best_explanation_belief = (Judgment)this.nars.inferenceEngine.localRules.Choice(belief, best_explanation_belief);
                    }
                }
            }

            count++;
        }
        if (best_explanation_belief == null)
        {
            Item<Concept> item = statement_concept.explanation_links.peek();
            best_explanation_belief = item.obj.belief_table.peek_random();
        }

        return best_explanation_belief;
    }*/

    /*    def get_prediction_preferred_with_true_postcondition(j)
        {
            *//*
    Gets the best explanation belief for the given sentence's statement
    that the sentence == able to interact with
    :param statement_concept:
    :return:
    *//*
            Concept statement_concept = this.peek_concept(j.statement); // B
            if (statement_concept.prediction_links.GetCount() == 0) return;
            best_prediction_belief = null;
            count = 0;
            MAX_ATTEMPTS = WorldConfig.NUMBER_OF_ATTEMPTS_TO_SEARCH_FOR_SEMANTICALLY_RELATED_BELIEF;
            while count < MAX_ATTEMPTS:
                item = statement_concept.prediction_links.peek();
        prediction_concept: Concept = item.obj; // A =/> B

            if prediction_concept.term.get_predicate_term().contains_positive(){
                // (A &/ B) =/> C && A.
                belief = prediction_concept.belief_table.peek_highest_confidence_interactable(j);
                if belief == null:
                        continue;
                    else if best_prediction_belief == null:
                        best_prediction_belief = belief;
                break;
                count += 1;

                if best_prediction_belief == null:
                item = statement_concept.prediction_links.peek();
                best_prediction_belief = item.obj.belief_table.peek_random();

                return best_prediction_belief;
            }

            def get_random_bag_prediction(j)
            {
            *//*
    Gets the best explanation belief for the given sentence's statement
    that the sentence == able to interact with
    :param statement_concept:
    :return:
    *//*
            statement_concept: Concept = this.peek_concept(j.statement); // B
                if len(statement_concept.prediction_links) == 0: return null;

            prediction_concept_item = statement_concept.prediction_links.peek();
            prediction_concept = prediction_concept_item.obj;
            prediction_belief = prediction_concept.belief_table.peek();

            return prediction_belief;
        }

        def get_random_bag_explanation(j)
        {
        *//*
    Gets the best explanation belief for the given sentence's statement
    that the sentence == able to interact with
    :param statement_concept:
    :return:
    *//*
        concept: Concept = this.peek_concept(j.statement); // B
            if len(concept.explanation_links) == 0: return null;

            explanation_concept_item = concept.explanation_links.peek();
            explanation_concept = explanation_concept_item.obj;
            explanation_belief = explanation_concept.belief_table.peek_random();

            return explanation_belief;
        }

        def get_random_explanation_preferred_with_true_precondition(j)
        {
            *//*
    Returns random explanation belief
    :param j:
    :return:
    *//*
            concept = this.peek_concept(j.statement);
            best_belief = null;
            count = 0;
            MAX_ATTEMPTS = WorldConfig.NUMBER_OF_ATTEMPTS_TO_SEARCH_FOR_SEMANTICALLY_RELATED_BELIEF;
            while count < MAX_ATTEMPTS:
                explanation_concept_item = concept.explanation_links.peek();
            explanation_concept = explanation_concept_item.obj;
            if len(explanation_concept.belief_table) == 0: continue;
            belief = explanation_concept.belief_table.peek();

            if belief != null:
                    if best_belief == null:
                        best_belief = belief;
                    else:
                        belief_is_pos_conj = TermConnectorMethods.is_conjunction(
                            belief.statement.get_subject_term().connector) && belief.statement.get_subject_term().contains_positive();

            best_belief_is_pos_conj = TermConnectorMethods.is_conjunction(
                best_belief.statement.get_subject_term().connector) && best_belief.statement.get_subject_term().contains_positive();

            if belief_is_pos_conj && not best_belief_is_pos_conj:
            best_belief = belief;
            else if best_belief_is_pos_conj && not belief_is_pos_conj:
                //
                        else:
                            best_belief = Local.Choice(best_belief, belief); // new best belief?

            count += 1;

            return best_belief;
        }


        def get_best_prediction(j)
        {
            *//*
    Returns the best prediction belief for a given belief
    :param j:
    :return:
    *//*
            concept = this.peek_concept(j.statement);
            best_belief = null;
            for prediction_concept_item in concept.prediction_links:


    prediction_concept = prediction_concept_item.obj;
            if len(prediction_concept.belief_table) == 0: continue;
            prediction_belief = prediction_concept.belief_table.peek();

            if prediction_belief != null:
                    if best_belief == null:
                        best_belief = prediction_belief;
                    else:
                        best_belief = Local.Choice(best_belief, prediction_belief); // new best belief?

            return best_belief;
        }

        def get_best_explanation_with_true_precondition(j)
        {
            *//*
    Returns the best prediction belief for a given belief
    :param j:
    :return:
    *//*
            concept = this.peek_concept(j.statement);
            best_belief = null;
            for concept_item in concept.explanation_links:

        concept = concept_item.obj;
                if len(concept.belief_table) == 0: continue;
            belief = concept.belief_table.peek();

            if belief != null &&\
                    TermConnectorMethods.is_conjunction(belief.statement.get_subject_term().connector) &&\
                    belief.statement.get_subject_term().contains_positive(){
                if best_belief == null:
                        best_belief = belief;
                    else:
                        best_belief = Local.Choice(best_belief, belief); // new best belief?

                return best_belief;

            }


            def get_prediction_with_desired_postcondition(statement_concept)
            {
                *//*
    Returns the best prediction belief && && highest desired postcondition for a given belief
    :param j:
    :return:
    *//*
                prediction_links = statement_concept.prediction_links;
                if len(prediction_links) == 0: return null;
            best_prediction_belief = null;
            count = 0;
            MAX_ATTEMPTS = WorldConfig.NUMBER_OF_ATTEMPTS_TO_SEARCH_FOR_SEMANTICALLY_RELATED_BELIEF;
            while count < MAX_ATTEMPTS:
                item = prediction_links.peek();
        prediction_concept: Concept = item.obj;  // A =/> B

            if this.peek_concept(prediction_concept.term.get_predicate_term()).is_desired(){
                // (A &/ B) =/> C && A.
                belief = prediction_concept.belief_table.peek();
                if belief != null:
                        if best_prediction_belief == null:
                            best_prediction_belief = belief;
                        else:
                            best_prediction_belief = Local.Choice(best_prediction_belief, belief);  // new best belief?

                count += 1;

                return best_prediction_belief;
            }

            def get_random_positive_prediction(j)
            {
                *//*
    Returns a random positive prediction belief for a given belief
    :param j:
    :return:
    *//*
                concept = this.peek_concept(j.statement);
                positive_beliefs = [];
                for prediction_concept_item in concept.prediction_links:
                prediction_concept = prediction_concept_item.obj;
            if len(prediction_concept.belief_table) == 0: continue;
            prediction_belief = prediction_concept.belief_table.peek();

            if prediction_belief != null:
                    if prediction_belief.is_positive(){
                positive_beliefs.append(prediction_belief);

                if len(positive_beliefs) == 0:
                return null;
                return positive_beliefs[round(random.random() * (len(positive_beliefs) - 1))]
                                                                        }

            def get_random_prediction(j)
            {
                *//*
    Returns a random positive prediction belief for a given belief
    :param j:
    :return:
    *//*
                concept = this.peek_concept(j.statement);
                if len(concept.prediction_links) == 0:
                return null;
            prediction_concept = concept.prediction_links.peek().obj;
            if len(prediction_concept.belief_table) == 0:
                return null;
            return prediction_concept.belief_table.peek();
        }

        def get_all_positive_predictions(j)
        {
            predictions = [];
            concept = this.peek_concept(j.statement);
            for prediction_concept_item in concept.prediction_links:

            prediction_concept = prediction_concept_item.obj;
            if len(prediction_concept.belief_table) == 0: continue;
            prediction_belief = prediction_concept.belief_table.peek();

            if prediction_belief != null:
                    if isinstance(prediction_belief.statement.get_predicate_term(), StatementTerm) && prediction_belief.is_positive(){
                predictions.append(prediction_belief);

                return predictions
                                                                            }

            public Judgment get_best_positive_desired_prediction(concept)
            {
                *//*
    Returns the best predictive implication from a given concept's prediction links,
    but only accounts those predictions whose postconditions are desired
    :param j:
    :return:
    *//*
                Judgment? best_belief = null;
                for (prediction_concept_item in concept.prediction_links)
                {
                    prediction_concept = prediction_concept_item.obj;
                    if (prediction_concept.belief_table.GetCount() == 0) continue;
                    prediction_belief = prediction_concept.belief_table.peek();

                    if prediction_belief != null && prediction_concept.is_positive(){
                        postcondition_term = prediction_concept.term.get_predicate_term();
                        if isinstance(postcondition_term, StatementTerm){
                            if this.peek_concept(postcondition_term).is_desired(){
                                if best_belief == null:
                                best_belief = prediction_belief;
                            else:
                                best_belief = Local.Choice(best_belief, prediction_belief); // new best belief?

            return best_belief;
        }
    }*/


    public int get_next_stamp_id()
    {
        /*
            :return: next available Stamp ID
        */
        this.next_stamp_id++;
        return this.next_stamp_id--;
    }


    public int get_next_percept_id()
    {
        /*
            :return: next available Percept ID
        */
        this.next_percept_id++;
        return this.next_percept_id--;
    }

}

public class Concept
{
    public NARS nars;
    /*
        NARS Concept
    */
    public Term term;  // concept's unique term
    public Bag<Concept> term_links;  // Bag of related concepts (related by term)
    public Bag<Concept> subterm_links;  // Bag of related concepts (related by term)
    public Bag<Concept> superterm_links;  // Bag of related concepts (related by term)
    public Table<Judgment> belief_table;
    public Table<Goal> desire_table;
    public Bag<Concept> prediction_links;
    public Bag<Concept> explanation_links;



    public Concept(Term term, NARS nars)
    {
        Asserts.assert_term(term);
        this.nars = nars;

        this.term = term;  // concept's unique term
        int granularity = nars.config.BAG_GRANULARITY;
        this.term_links = new Bag<Concept>(this.nars.config.CONCEPT_LINK_CAPACITY, granularity);  // Bag of related concepts (related by term)
        this.subterm_links = new Bag<Concept>(this.nars.config.CONCEPT_LINK_CAPACITY, granularity);  // Bag of related concepts (related by term)
        this.superterm_links = new Bag<Concept>(this.nars.config.CONCEPT_LINK_CAPACITY, granularity);  // Bag of related concepts (related by term)
        this.belief_table = new Table<Judgment>(this.nars.config.TABLE_DEFAULT_CAPACITY, nars);
        this.desire_table = new Table<Goal>(this.nars.config.TABLE_DEFAULT_CAPACITY, nars);
        this.prediction_links = new Bag<Concept>(this.nars.config.CONCEPT_LINK_CAPACITY, granularity);
        this.explanation_links = new Bag<Concept>(this.nars.config.CONCEPT_LINK_CAPACITY, granularity);
    }


    public override string ToString()
    {
        return this.get_term_string();
    }

    public Term get_term()
    {
        return this.term;
    }

    public bool is_desired()
    {
        /*
        :return: If the highest-confidence belief says this statement == true
        */
        if (this.desire_table.GetCount() == 0) return false;
        return this.nars.inferenceEngine.localRules.Decision(this.desire_table.peek());
    }

    public bool is_positive()
    {
        /*
            :return: If the highest-confidence belief says this statement == true
        */
        if (this.belief_table.GetCount() == 0) return false;
        return this.nars.inferenceEngine.is_positive(this.belief_table.peek());
    }

    public float? get_expectation()
    {
        /*
            :return: If the highest-confidence belief says this statement is true
        */
        if (this.belief_table.GetCount() == 0) return null;
        Judgment belief = this.belief_table.peek();
        return this.nars.inferenceEngine.get_expectation(belief);
    }

    public void set_term_links(Concept subterm_concept)
    {
        /*
        Set a bidirectional term link between 2 concepts && the subterm/superterm link
        Does nothing if the link already exists

        :param subterm concept to this superterm concept (this)
        */
        if (this.term_links.Contains(subterm_concept)) return;  // already linked

        // add to term links
        Item<Concept> item = this.term_links.PUT_NEW(subterm_concept);
        this.term_links.change_priority(item.key, 0.5f);

        item = subterm_concept.term_links.PUT_NEW(this);
        subterm_concept.term_links.change_priority(item.key, 0.5f);

        // add to subterm links
        item = this.subterm_links.PUT_NEW(subterm_concept);
        this.subterm_links.change_priority(item.key, 0.5f);

        // add to superterm links
        item = subterm_concept.superterm_links.PUT_NEW(this);
        subterm_concept.superterm_links.change_priority(item.key, 0.5f);
    }

    public void remove_term_link(Concept concept)
    {
        /*
            Remove a bidirectional term link between this concept && another concept
            todo: use this somewhere
        */
        Asserts.assert_concept(concept);
        Asserts.assert(this.term_links.Contains(concept), concept + "must be in term links.");
        this.term_links.TAKE_USING_KEY(Item<Concept>.get_key_from_object(concept));
        concept.term_links.TAKE_USING_KEY(Item<Concept>.get_key_from_object(this));
    }

    public void set_prediction_link(Concept concept)
    {
        /*
            Set a prediction link between 2 concepts
            Does nothing if the link already exists
        */
        if (concept == null) return;
        Asserts.assert_concept(concept);
        if (this.prediction_links.Contains(concept)) return;  // already linked
        Item<Concept> concept_item = this.prediction_links.PUT_NEW(concept);
        this.prediction_links.change_priority(concept_item.key, 0.99f);
    }

    public void remove_prediction_link(Concept concept)
    {
        /*
            Remove a bidirectional term link between this concept && another concept
            todo: use this somewhere
        */
        Asserts.assert(this.prediction_links.Contains(concept), concept + "must be in prediction links.");
        this.prediction_links.TAKE_USING_KEY(Item<Concept>.get_key_from_object(concept));
    }

    public void set_explanation_link(Concept concept)
    {
        /*
            Set an explanation between 2 concepts
            Does nothing if the link already exists
        */
        return; //todo remove;
        if (this.explanation_links.Contains(concept)) return;  // already linked
        Item<Concept> concept_item = this.explanation_links.PUT_NEW(concept);
        this.explanation_links.change_priority(concept_item.key, 0.99f);
    }


    public void remove_explanation_link(Concept concept)
    {
        /*
        Remove a bidirectional term link between this concept && another concept
        todo: use this somewhere
        */
        Asserts.assert(this.explanation_links.Contains(concept), concept + "must be in prediction links.");
        this.explanation_links.TAKE_USING_KEY(Item<Concept>.get_key_from_object(concept));
    }

    public string get_term_string()
    {
        /*
        A concept is named by its term
        */
        return this.term.get_term_string();
    }
}