/*
    Author: Christian Hahm
    Created: May 27, 2022
    Purpose: NARS definition
*/


using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NARS
{
    /*
       NARS Class
    */
    public NARSAgent agent;

    public NARSConfig config;

    public NARSInferenceEngine inferenceEngine;
    public Memory memory;
    public Buffer<Sentence> global_buffer;
    public TemporalModule<Judgment> temporal_module;

    public HelperFunctions helperFunctions;

    public int current_cycle_number;

    List<(int, StatementTerm, float, List<string>)> operation_queue; // operations the system has queued to executed
    Goal current_operation_goal_sequence;
    string last_executed = "";
    

    // enforce milliseconds per working cycle
    int cycle_begin_time = 0;

    // keeps track of number of working cycles per second
    int cycles_per_second_timer = 0;
    int last_working_cycle = 0;

    public NARS(NARSAgent agent)
    {
        this.agent = agent;
        this.config = new NARSConfig();
        this.inferenceEngine = new NARSInferenceEngine(this);
        this.memory = new Memory(this.config.MEMORY_CONCEPT_CAPACITY, this);

        this.helperFunctions = new HelperFunctions(this);

        this.global_buffer = new Buffer<Sentence>(this.config.GLOBAL_BUFFER_CAPACITY);
        this.temporal_module = new TemporalModule<Judgment>(this, this.config.EVENT_BUFFER_CAPACITY);
        //this.vision_buffer = new SpatialBuffer(dimensions = WorldConfig.VISION_DIMENSIONS)


        this.operation_queue = new List<(int, StatementTerm, float, List<string>)>(); // operations the system has queued to executed

        //this.memory.conceptualize_term(Global.Global.TERM_SELF);
    }




    public void run()
    {
        /*
            Infinite loop of working cycles
        */
        while (true)
        {
            // global parameters
            if (this.config.paused)
            {
                Thread.Sleep(1000);
                continue;
            }

            // time.sleep(0.2)
            this.do_working_cycle();
        }
    }


    public void do_working_cycle()
    {
        /*
            Performs 1 working cycle.
            In each working cycle, NARS either *Observes* OR *Considers*:
        */

        //time.sleep(0.1)
        this.current_cycle_number++;


        // process input channel && temporal module
        //InputChannel.process_input_channel()

        // OBSERVE
        this.Observe();

        // todo begin spatial take vvv

        /*        Sentence vision_sentence = this.vision_buffer.take(pooled=false)
                if vision_sentence == not null:
                    this.global_buffer.PUT_NEW(NARSDataStructures.Other.Task(vision_sentence))

                vision_sentence = this.vision_buffer.take(pooled=true)
                if vision_sentence == not null:
                    this.global_buffer.PUT_NEW(NARSDataStructures.Other.Task(vision_sentence))
        */
        // todo end spatial take ^^

        // global buffer
        int buffer_len = this.global_buffer.GetCount();
        int tasks_left = buffer_len;
        while (tasks_left > 0)
        {
            Sentence buffer_item = this.global_buffer.take().obj;
            // process task
            this.process_sentence_initial(buffer_item);
            tasks_left--;
        }


        // now execute operations
        this.execute_operation_queue();

/*                #todo this.temporal_module.process_anticipations()

                // debug statements
                if(this.nars.config.DEBUG)
                    Debug.Log("operation queue: " + str(len(this.operation_queue)))
                    Debug.Log("anticipations queue: " + str(len(this.temporal_module.anticipations_queue)))
                    Debug.Log("global buffer: " + str(len(this.global_buffer)))


                if WorldConfig.USE_PROFILER:
                    pstats.Stats(this.pr).sort_stats('tottime').print_stats(10) #tottime == time spent in the function alone, cumtime == including subfunctions
                    this.pr.enable()
        */
    }


    public void do_working_cycles(int cycles)
    {
        /*
            Performs the given number of working cycles.
        */
        for (int i = 0; i < cycles; i++)
        {
            this.do_working_cycle();
        }
    }

    public void Observe()
    {
        /*
            Process a task from the global buffer.

            This function should never produce new tasks.
        */
        //
    }

    public void Consider(Concept? concept = null)
    {
        /*
            Process a belief from a random concept in memory.

            This function can result in new tasks

            :param: concept: concept to consider. If null, picks a random concept
        */
        Item<Concept>? concept_item = null;
        if (concept == null)
        {
            concept_item = this.memory.get_random_concept_item();
            if (concept_item == null) return; // nothing to ponder
            concept = concept_item.obj;
        }


        // If concept is not named by a statement, get a related concept that is a statement
        int attempts = 0;
        int max_attempts = 2;
        while (attempts < max_attempts && !(((concept.term is StatementTerm) || ((concept.term is CompoundTerm) && !((CompoundTerm)concept.term).is_first_order()))))
        {
            if (concept.term_links.GetCount() > 0)
            {
                concept = concept.term_links.peek().obj;
            }
            else
            {
                break;
            }

            attempts += 1;
        }

        // debugs
        if (this.config.DEBUG)
        {
            string str = "Considering concept: " + concept.term.ToString();
            if (concept_item != null) str += concept_item.budget.ToString();
            if (concept.belief_table.GetCount() > 0) str += " expectation: " + this.inferenceEngine.get_expectation(concept.belief_table.peek()).ToString();
            if (concept.desire_table.GetCount() > 0) str += " desirability: " + this.inferenceEngine.get_desirability(concept.desire_table.peek()).ToString();
            Debug.Log(str);
        }

        //Debug.Log("CONSIDER: " + str(concept))

        if (concept != null && attempts != max_attempts)
        {
            // process a belief && desire
            if (concept.belief_table.GetCount() > 0)
            {
                this.process_judgment_continued(concept.belief_table.peek());  // process most confident belief
            }


            if (concept.desire_table.GetCount() > 0)
            {
                this.process_goal_continued(concept.desire_table.peek()); // process most confident goal
            }


            // decay priority;
            if (concept_item != null)
            {
                this.memory.concepts_bag.decay_item(concept_item.key, this.config.PRIORITY_DECAY_VALUE);
            }
        }
    }



    /*    public void save_memory_to_disk(string filename= "memory1.nars") {
            *//*
                Save the NARS Memory instance to disk
            *//*
            int old_limit = sys.getrecursionlimit();
            sys.setrecursionlimit(old_limit * 2);
            with open(filename, "wb") as f:
                Global.Global.print_to_output("SAVING SYSTEM MEMORY TO FILE: " + filename);
            try:
                    pickle.dump(this.memory, f, pickle.HIGHEST_PROTOCOL);
            Global.Global.print_to_output("SAVE MEMORY SUCCESS");
        except:
            Global.Global.print_to_output("SAVE MEMORY FAILURE");
            sys.setrecursionlimit(old_limit);
            }

        public void load_memory_from_disk(string filename= "memory1.nars") {
                *//*
                    Load a NARS Memory instance from disk.
                    This will override the NARS' current memory
                *//*
                try {
                    with open(filename, "rb") as f:
                    Global.Global.print_to_output("LOADING SYSTEM MEMORY FILE: " + filename);
            // load memory from file
            this.memory = pickle.load(f);
            // Print memory contents to internal data GUI
            if WorldConfig.GUI_USE_INTERFACE:
                        Global.Global.clear_output_gui(data_structure = this.memory.concepts_bag);
            for item in this.memory.concepts_bag:
                            if item not in this.memory.concepts_bag:
                                Global.Global.print_to_output(msg = str(item), data_structure = this.memory.concepts_bag);

            if WorldConfig.GUI_USE_INTERFACE:
                        NARSGUI.NARSGUI.gui_total_cycles_stringvar.set("Cycle #" + str(this.memory.this.nars.current_cycle_number));

            Debug.Log("LOAD MEMORY SUCCESS");
        } except{
            Debug.Log("LOAD MEMORY FAIL");
            }
                }
    */
    /*    def handle_gui_pipes(this){
            if Global.Global.NARS_object_pipe == null: return;

            // GUI
            Global.Global.NARS_string_pipe.send(("cycles", "Cycle #" + str(this.memory.this.nars.current_cycle_number), null, 0));


            while Global.Global.NARS_object_pipe.poll(){
                // for blocking communication only, when the sender expects a result.
                // This checks for a message request from the GUI
                (command, key, data_structure_id) = Global.Global.NARS_object_pipe.recv();
                if command == "getitem":
                    data_structure = null;
                    if data_structure_id == str(this.temporal_module){
                    data_structure = this.temporal_module;
                    Global.Global.NARS_object_pipe.send(null);
                    else if data_structure_id == str(this.memory.concepts_bag){
                        data_structure = this.memory.concepts_bag;
                        if data_structure == not null:
                            item: NARSDataStructures.ItemContainers.Item = data_structure.peek(key);
                            if item == null:
                                Global.Global.NARS_object_pipe.send(null);
                            else:
                                Global.Global.NARS_object_pipe.send(item.get_gui_info());

                else if command == "getsentence":
                    sentence_string = key;
                        statement_start_idx = sentence_string.find(NALSyntax.StatementSyntax.Start.value);
                        statement_end_idx = sentence_string.rfind(NALSyntax.StatementSyntax.End.value);
                        statement_string = sentence_string[statement_start_idx: statement_end_idx + 1];
                        term = from_string(statement_string);
                        concept_item = this.memory.peek_concept_item(term);
                    concept = concept_item.object;

                    if concept == null:
                        Global.Global.NARS_object_pipe.send(null);  // couldn't get concept, maybe it was purged
                    else:
                        punctuation_str = sentence_string[statement_end_idx + 1];
                        if punctuation_str == NALSyntax.Punctuation.Judgment.value:
                            table = concept.belief_table;
                        else if punctuation_str == NALSyntax.Punctuation.Goal.value:
                            table = concept.desire_table;
                        else:
                            assert false,"ERROR: Could not parse GUI sentence fetch";
                        ID = sentence_string[sentence_string.find(Global.Global.MARKER_ITEM_ID) + len(
                            Global.Global.MARKER_ITEM_ID){sentence_string.rfind(Global.Global.MARKER_ID_END)];
                            sent = false;
                        for knowledge_tuple in table:
                            knowledge_sentence = knowledge_tuple[0];
                            knowledge_sentence_str = str(knowledge_sentence);
                            knowledge_sentence_ID = knowledge_sentence_str[knowledge_sentence_str.find(Global.Global.MARKER_ITEM_ID) + len(
                                Global.Global.MARKER_ITEM_ID){knowledge_sentence_str.rfind(Global.Global.MARKER_ID_END)];
                            if ID == knowledge_sentence_ID:
                                Global.Global.NARS_object_pipe.send(("sentence", knowledge_sentence.get_gui_info()));
                                sent = true;
                                break
                        if !sent: Global.Global.NARS_object_pipe.send(("concept", concept_item.get_gui_info())); // couldn't get sentence, maybe it was purged
                                else if command == "getconcept":
                    item = this.memory.peek_concept_item(key);
                    if item != null:
                        Global.Global.NARS_object_pipe.send(item.get_gui_info());
                    else:
                        Global.Global.NARS_object_pipe.send(null);  // couldn't get concept, maybe it was purged

                                while Global.Global.NARS_string_pipe.poll(){
                                    // this pipe can hold as many tasks as needed
                                    (command, data) = Global.Global.NARS_string_pipe.recv()


                if command == "userinput":
                    InputChannel.parse_and_queue_input_string(data)
                else if command == "visualimage":
                    // user loaded image for visual input
                    img = data
                                        InputChannel.queue_visual_sensory_image_array(img)
                else if command == "visualimagelabel":
                    // user loaded image for visual input
                    label = data
                                        InputChannel.parse_and_queue_input_string("(" + label + "--> SEEN). :|:")
                else if command == "duration":
                    WorldConfig.TAU_WORKING_CYCLE_DURATION = data
                else if command == "paused":
                    Global.Global.paused = data
                                                            }*/


    public void process_sentence_initial(Sentence j)
    {
        /*
            Initial processing for a Narsese sentence
        */
        Term task_statement_term = j.statement;
        if (task_statement_term.contains_variable()) return; // todo handle variables

        // statement_concept_item = this.memory.peek_concept_item(task_statement_term)
        // statement_concept = statement_concept_item.object


        // get (|| create if necessary) statement concept, && sub-term concepts recursively
        if (j is Judgment)
        {
            this.process_judgment_initial((Judgment)j);
        }
        else if (j is Question)
        {
            this.process_question_initial((Question)j);
        }
        else if (j is Goal)
        {
            this.process_goal_initial((Goal)j);
        }

        //     if not task.sentence.is_event(){
        //         statement_concept_item.budget.set_quality(0.99)
        //         this.memory.concepts_bag.change_priority(key=statement_concept_item.key,
        //                                                  new_priority=0.99)

        // this.memory.concepts_bag.strengthen_item(key=statement_concept_item.key)
        //print("concept strengthen " + str(statement_concept_item.key) + " to " + str(statement_concept_item.budget))


    }


    public void process_judgment_initial(Judgment j)
    {
        /*
            Processes a Narsese Judgment Task
            Insert it into the belief table && revise it with another belief

            :param Judgment Task to process
        */
        if(j.is_event()){
            // only put non-derived atomic events in temporal module for now
            this.temporal_module.PUT_NEW(j);
        }

        Item<Concept> task_statement_concept_item = this.memory.peek_concept_item(j.statement);
        if (task_statement_concept_item == null) return;

        this.memory.concepts_bag.strengthen_item_quality(task_statement_concept_item.key);

        Concept statement_concept = task_statement_concept_item.obj;

        // todo commented out immediate inference because it floods the system
        // derived_sentences = []#NARSInferenceEngine.do_inference_one_premise(j)
        // for derived_sentence in derived_sentences:
        //    this.global_buffer.put_new(NARSDataStructures.Other.Task(derived_sentence))

        // if j.is_event(){
        //     // anticipate event j
        //     pass #todo this.temporal_module.anticipate_from_event(j)

        statement_concept.belief_table.put(j);

        Judgment current_belief = statement_concept.belief_table.peek();
        this.process_judgment_continued(current_belief);

        if (this.config.DEBUG)
        {
            string str = "Integrated new BELIEF Task: " + j.ToString() + "from ";
            foreach (Sentence premise in j.stamp.parent_premises)
            {
                str += premise.ToString() + ",";
            }
            Debug.Log(str);
        }

    }

    public void process_judgment_continued(Judgment j1, bool revise = true)
    {
        /*
            Continued processing for Judgment

            :param j1: Judgment
            :param related_concept: concept related to judgment with which to perform semantic inference
        */
        if (this.config.DEBUG)
        {
            Debug.Log("Continued Processing JUDGMENT: " + j1.ToString());
        }

        // get terms from sentence
        Term statement_term = j1.statement;

        // do regular semantic inference;
        List<Sentence> results = this.process_sentence_semantic_inference(j1);
        foreach (Sentence result in results)
        {
            this.global_buffer.PUT_NEW(result);
        }
    }


    public void process_question_initial(Question j)
    {
        Item<Concept> task_statement_concept_item = this.memory.peek_concept_item(j.statement);
        if (task_statement_concept_item == null) return;

        this.memory.concepts_bag.strengthen_item_quality(task_statement_concept_item.key);

        Concept task_statement_concept = task_statement_concept_item.obj;
        // get the best answer from concept belief table
        Judgment best_answer = task_statement_concept.belief_table.peek();
        Sentence? j1 = null;
        if (best_answer != null)
        {
            // Answer the question
            if (j.is_from_input && j.needs_to_be_answered_in_output)
            {
                Debug.Log("OUT: " + best_answer.ToString());
                j.needs_to_be_answered_in_output = false;
            }

            // do inference between answer && a related belief
            j1 = best_answer;
        }
        else
        {
            // do inference between question && a related belief
            j1 = j;
        }

        this.process_sentence_semantic_inference(j1);

    }


    public void process_goal_initial(Goal j)
    {
        /*
            Processes a Narsese Goal Task

            :param Goal Task to process
        */

        /*
            Initial Processing

            Insert it into the desire table || revise with the most confident desire
        */
        Item<Concept> statement_concept_item = this.memory.peek_concept_item(j.statement);
        Concept statement_concept = statement_concept_item.obj;
        this.memory.concepts_bag.change_quality(statement_concept_item.key, 0.999f);

        // store the most confident desire
        statement_concept.desire_table.put(j);

        Goal current_desire = statement_concept.desire_table.peek();

        this.process_goal_continued(current_desire);

        if (this.config.DEBUG)
        {
            string str = "Integrated new GOAL Task: " + j.ToString() + "from ";
            foreach (Sentence premise in j.stamp.parent_premises)
            {
                str += premise.ToString() + ",";
            }
            Debug.Log(str);
        }
    }

    public void process_goal_continued(Goal j)
    {
        /*
            Continued processing for Goal

            :param j: Goal
            :param related_concept: concept related to goal with which to perform semantic inference
        */
        if (this.config.DEBUG) Debug.Log("Continued Processing GOAL: " + j.ToString());

        Term statement = j.statement;

        Concept statement_concept = this.memory.peek_concept(statement);

        // see if it should be pursued
        bool should_pursue = this.inferenceEngine.localRules.Decision(j);
        if (!should_pursue)
        {
            //Debug.Log("Goal failed decision-making rule " + j.ToString())
            if (this.config.DEBUG && statement.is_op())
            {
                Debug.Log("Operation failed decision-making rule " + j.ToString());

            }
            return;  // Failed decision-making rule
        }
        else
        {
            //Debug.Log("Goal passed decision-making rule " + j.ToString());
        }


        // at this point the system wants to pursue this goal.
        // now check if it should be inhibited (negation == more highly desired).
        // negated_statement = j.statement.get_negated_term()
        // negated_concept = this.memory.peek_concept(negated_statement)
        // if len(negated_concept.desire_table) > 0:
        //     desire = j.get_expectation()
        //     neg_desire = negated_concept.desire_table.peek().get_expectation()
        //     should_inhibit = neg_desire > desire
        //     if should_inhibit:
        //         Debug.Log("Event was inhibited " + j.get_term_string())
        //         return  // Failed inhibition decision-making rule
        if (statement.is_op() && j.statement.connector != TermConnector.Negation)
        {
            //if not j.executed:
            this.queue_operation(j);
            //    j.executed = false
        }
        else
        {
            // check if goal already achieved
            Judgment? desire_event = statement_concept.belief_table.peek();
            if (desire_event != null)
            {
                if (this.inferenceEngine.is_positive(desire_event))
                {
                    Debug.Log(desire_event.ToString() + " is positive for goal: " + j.ToString());
                    return;  // Return if goal is already achieved
                }
            }

            if (statement is CompoundTerm)
            {
                if (TermConnectorMethods.is_conjunction(statement.connector))
                {
                    // if it's a conjunction (A &/ B), simplify using true beliefs (e.g. A)
                    Term subterm = ((CompoundTerm)statement).subterms[0];
                    Concept subterm_concept = this.memory.peek_concept(subterm);
                    Judgment? belief = subterm_concept.belief_table.peek();
                    if (belief != null && this.inferenceEngine.is_positive(belief))
                    {
                        // the first component of the goal is positive, do inference && derive the remaining goal component
                        List<Sentence> results = this.inferenceEngine.do_semantic_inference_two_premise(j, belief);
                        foreach (Sentence result in results)
                        {
                            this.global_buffer.PUT_NEW(result);
                        }
                        return; // done deriving goals
                    }
                    else
                    {
                        if (this.config.DEBUG) Debug.Log(subterm_concept.term.ToString() + " was not positive to split conjunction.");
                    }
                }
                else if (statement.connector == TermConnector.Negation && TermConnectorMethods.is_conjunction(((CompoundTerm)statement).subterms[0].connector))
                {
                    // if it's a negated conjunction (--,(A &/ B))!, simplify using true beliefs (e.g. A.)
                    // (--,(A &/ B)) ==> D && A
                    // induction
                    // :- (--,(A &/ B)) && A ==> D :- (--,B) ==> D :- (--,B)!
                    CompoundTerm conjunction = (CompoundTerm)((CompoundTerm)statement).subterms[0];
                    Term subterm = conjunction.subterms[0];
                    Concept subterm_concept = this.memory.peek_concept(subterm);
                    Judgment belief = subterm_concept.belief_table.peek();
                    if (belief != null && this.inferenceEngine.is_positive(belief))
                    {
                        // the first component of the goal == negative, do inference && derive the remaining goal component
                        List<Sentence> results = this.inferenceEngine.do_semantic_inference_two_premise(j, belief);
                        foreach (Sentence result in results)
                        {
                            this.global_buffer.PUT_NEW(result);
                        }

                        return; // done deriving goals
                    }
                }
            }

            // random_belief = null
            // contextual_belief = null
            // if len(statement_concept.explanation_links) > 0 && j.statement.connector != NALSyntax.TermConnector.Negation:
            //     // process with random && context-relevant explanation A =/> B
            //     random_belief = this.memory.get_random_bag_explanation(j) // (E =/> G)
            //     #contextual_belief = this.memory.get_best_explanation_with_true_precondition(j)
            // else if len(statement_concept.prediction_links) > 0 && j.statement.connector == NALSyntax.TermConnector.Negation:
            //     random_belief = this.memory.get_random_bag_prediction(j) // ((--,G) =/> E)
            //     #contextual_belief = this.memory.get_prediction_preferred_with_true_postcondition(j) // ((--,G) =/> E)

            // if random_belief == not null:
            //      if Config.DEBUG:Debug.Log(str(random_belief) + " == random explanation for " + str(j))
            //      // process goal with explanation
            //      results = NARSInferenceEngine.do_semantic_inference_two_premise(j, random_belief)
            //      for result in results:
            //          this.global_buffer.put_new(NARSDataStructures.Other.Task(result))

            //      this.process_judgment_sentence(random_belief)


            // if contextual_belief == not null:
            //     if Config.DEBUG: Debug.Log(str(contextual_belief) + " == contextual explanation for " + str(j))
            //     // process goal with explanation
            //     results = NARSInferenceEngine.do_semantic_inference_two_premise(j, contextual_belief)
            //     for result in results:
            //         this.global_buffer.put_new(NARSDataStructures.Other.Task(result))

            //     this.process_judgment_sentence(contextual_belief)

            // else:
            //     if Config.DEBUG: Debug.Log("No contextual explanations for " + str(j))
        }
    }



    public List<Sentence> process_sentence_semantic_inference(Sentence j1, Concept? related_concept = null)
    {
        /*
            Processes a Sentence with a belief from a related concept.

            :param j1 - sentence to process
            :param related_concept - (Optional) concept from which to fetch a belief to process the sentence with

            #todo handle variables
        */
        List<Sentence> results = new List<Sentence>();
        if (this.config.DEBUG) Debug.Log("Processing: " + j1.ToString());
        Term statement_term = j1.statement;
        // get (or create if necessary) statement concept, and sub-term concepts recursively
        Concept statement_concept = this.memory.peek_concept(statement_term);

        if (related_concept == null)
        {
            if (this.config.DEBUG) Debug.Log("Processing: Peeking randomly related concept");

            if (statement_term is CompoundTerm)
            {
                if (statement_concept.prediction_links.GetCount() > 0)
                {
                    related_concept = statement_concept.prediction_links.peek().obj;
                }
            }
            else if (statement_term is StatementTerm && !((StatementTerm)statement_term).is_first_order())
            {

                // subject_term = statement_term.get_subject_term()
                // related_concept = this.memory.peek_concept(subject_term)
            }
            else if (statement_term is StatementTerm && ((StatementTerm)statement_term).is_first_order() && j1.is_event())
            {
                if (statement_concept.explanation_links.GetCount() > 0)
                {
                    related_concept = statement_concept.explanation_links.peek().obj;
                }
                else if (statement_concept.superterm_links.GetCount() > 0)
                {
                    related_concept = statement_concept.superterm_links.peek().obj;
                }
            }
            else
            {
                related_concept = this.memory.get_semantically_related_concept(statement_concept);
            }

            if (related_concept == null) return results;
        }
        else
        {
            Debug.Log("Processing: Using related concept " + related_concept.ToString());
        }


        // check for a belief we can interact with
        Sentence j2 = related_concept.belief_table.peek();

        if (j2 == null)
        {
            if (this.config.DEBUG) Debug.Log("No related beliefs found for " + j1.ToString());
            return results;  // done if can't interact
        }

        results = this.inferenceEngine.do_semantic_inference_two_premise(j1, j2);

        // check for a desire we can interact with
        j2 = related_concept.desire_table.peek_random();

        if (j2 == null)
        {
            if (this.config.DEBUG) Debug.Log("No related goals found for " + j1.ToString());
            return results; // done if can't interact
        }

        results.AddRange(this.inferenceEngine.do_semantic_inference_two_premise(j1, j2));

        return results;
    }

    /*
        OPERATIONS
    */

    public void queue_operation(Goal operation_goal)
    {
        /*
            Queue a desired operation.
            Can be an atomic operation or a compound.
        :param operation_goal: Including SELF, arguments, and Operation itself
        :return:
        */
        // todo extract && use args
        if (this.config.DEBUG) Debug.Log("Attempting queue operation: " + operation_goal.ToString());
        // full_operation_term.get_subject_term()
        Term operation_statement = operation_goal.statement;
        float desirability = this.inferenceEngine.get_desirability(operation_goal);

        if (this.current_operation_goal_sequence != null)
        {
            // in the middle of a operation sequence already
            Goal better_goal = (Goal)this.inferenceEngine.localRules.Choice(operation_goal, this.current_operation_goal_sequence);
            if (better_goal == this.current_operation_goal_sequence) return; // don't execute since the current sequence == more desirable
                                                                             // else, the given operation == more desirable
            this.operation_queue.Clear();
        }

        if (this.config.DEBUG) Debug.Log("Queueing operation: " + this.helperFunctions.sentence_to_string(operation_goal));

        List<string> parent_strings = new List<string>();
        // create an anticipation if this goal was based on a higher-order implication
        foreach (Sentence parent in operation_goal.stamp.parent_premises)
        {
            parent_strings.Add(this.helperFunctions.sentence_to_string(parent));
        }

        // insert operation into queue to be execute after the interval
        // intervals of zero will result in immediate execution (assuming the queue is processed afterwards && in the same cycle as this function)
        if (operation_statement is StatementTerm)
        {
            // atomic op
            this.current_operation_goal_sequence = operation_goal;
            this.operation_queue.Add((0, (StatementTerm)operation_statement, desirability, parent_strings));
        }
        else if (operation_statement is CompoundTerm)
        {
            List<Term> subterms = ((CompoundTerm)operation_statement).subterms;
            // higher-order operation like A &/ B or A &| B
            int atomic_ops_left_to_execute = subterms.Count;
            this.current_operation_goal_sequence = operation_goal;

            int working_cycles = 0;
            int num_of_ops = subterms.Count;
            Term subterm;
            for (int i = 0; i < num_of_ops; i++)
            {
                // insert the atomic subterm operations and their working cycle delays
                subterm = subterms[i];
                this.operation_queue.Add((working_cycles, (StatementTerm)subterm, desirability, parent_strings));
                if (i < num_of_ops - 1)
                {
                    working_cycles += this.helperFunctions.convert_from_interval(((CompoundTerm)operation_statement).intervals[i]);
                }
            }

        }

        if(this.config.DEBUG) Debug.Log("Queued operation: " + operation_statement.ToString());
    }


    public void execute_operation_queue()
    {
        /*
            Loop through all operations && decrement their remaining interval delay.
            If delay is zero, execute the operation
        :return:
        */
        this.last_executed = null;
        int i = 0;
        while (i < this.operation_queue.Count)
        {
            (int remaining_working_cycles, StatementTerm operation_statement, float desirability, List<string> parents) = this.operation_queue[i];

            if (remaining_working_cycles == 0)
            {
                // operation is ready to execute
                this.execute_atomic_operation(operation_statement, desirability, parents);
                // now remove it from the queue
                this.operation_queue.RemoveAt(i);
                this.last_executed = operation_statement.ToString();
                i -= 1;
            }
            else
            {
                // decrease remaining working cycles
                this.operation_queue[i] = (this.operation_queue[i].Item1-1, this.operation_queue[i].Item2, this.operation_queue[i].Item3, this.operation_queue[i].Item4);
            }
            i += 1;
        }

        if (this.operation_queue.Count == 0) this.current_operation_goal_sequence = null;
    }


    public void execute_atomic_operation(StatementTerm operation_statement_to_execute, float desirability, List<string> parents)
    {
        Concept statement_concept = this.memory.peek_concept(operation_statement_to_execute);

        // execute an atomic operation immediately
        string predicate_str = operation_statement_to_execute.get_predicate_term().ToString();
        int current_cycle = this.current_cycle_number;
        string str = "EXE: ^" + predicate_str +
            " cycle #" + current_cycle +
            " based on desirability: " + desirability.ToString();

        Debug.Log(str);

        this.agent.SendMotorOutput(operation_statement_to_execute);
        

        // input the operation statement
        Judgment operation_event = new Judgment(operation_statement_to_execute, new EvidentialValue(), this.current_cycle_number);
        this.process_judgment_initial(operation_event);
    }
}