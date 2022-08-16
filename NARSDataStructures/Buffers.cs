/*
    Author: Christian Hahm
    Created: May 24, 2022
    Purpose: Holds data structure implementations that are specific / custom to NARS
*/
using Priority_Queue;
using System;
using System.Collections.Generic;


public class Buffer<T> : ItemContainer<T>
{
    PriorityQueue<Item<T>, float> priority_queue;

    public Buffer(int capacity) : base(capacity)
    {
        this.priority_queue = new PriorityQueue<Item<T>, float>(new DecendingComparer<float>());
    }

    public Item<T>? take() {
        /*
            Take the max priority item
            :return:
        */
        if(this.GetCount() == 0) return null;
        Item<T> item = this.priority_queue.Dequeue();
        this._take_from_lookup_dict(item.key);
        return item;
    }

    public Item<T>? peek(string? key) {
        /*
            Peek item with highest priority
            O(1)

            Returns None if depq is empty
        */
        if(this.GetCount() == 0) return null;
        if (key == null) {
            return this.priority_queue.First;
        }
        else {
            return base.peek_using_key(key);
        }

    }


    public override Item<T> PUT_NEW(T obj)
    {
        Item<T> item = base.PUT_NEW(obj);
        this.priority_queue.Enqueue(item, item.budget.get_priority());
        return item;
    }

    class DecendingComparer<TKey> : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            return y.CompareTo(x);
        }
    }
}


/*
public class SpatialBuffer {
    /*
        Holds the current sensation signals in a spatial layout / array.

        The data is converted to Narsese when extracted.
    /

    int[,] dimensions;

    public SpatialBuffer(int[,] dimensions){
        /*
        :param dimensions: dimensions of the 2d buffer as a tuple (y, x)
        /
        this.dimensions = dimensions
        this.array = np.full(shape=dimensions,
                             fill_value=EvidentialValue(0.0,0.9))
        this.components_bag = Bag(item_type=object,
                                  capacity=1000,
                                  granularity=100)

        this.pooled_array = np.full(shape=dimensions,
                             fill_value=EvidentialValue(0.0,0.9))
        this.pooled_components_bag = Bag(item_type=object,
                                  capacity=1000,
                                  granularity=100)

        this.last_taken_img_array = null
        this.last_sentence = null

        // initialize with uniform probabilility

    def blank_image(this){
        this.set_image(np.empty(shape=this.array.shape))

    def set_image(this, img){
        this.img = img
        original_event_array = this.transduce_raw_vision_array(img)

        // assert event_array.shape == this.dimensions,\
        //     "ERROR: Data dimensions are incompatible with Spatial Buffer dimensions " \
        //     + str(event_array.shape) + " && " + str(this.dimensions)

        this.array = np.array(original_event_array)
        this.components_bag.clear()

        maximum = 0
        for indices, sentence in np.ndenumerate(this.array){
            if sentence.value.frequency > WorldConfig.POSITIVE_THRESHOLD \
                    && not (isinstance(sentence.statement,
                                        CompoundTerm) && sentence.statement.connector == NALSyntax.TermConnector.Negation){
                maximum = max(maximum, sentence.value.frequency * sentence.value.confidence)

        for indices, sentence in np.ndenumerate(this.array){
            if sentence.value.frequency > WorldConfig.POSITIVE_THRESHOLD \
                && not (isinstance(sentence.statement,CompoundTerm) && sentence.statement.connector == NALSyntax.TermConnector.Negation){
                priority = sentence.value.frequency * sentence.value.confidence / maximum
                object = indices
                this.components_bag.PUT_NEW(object)
                this.components_bag.change_priority(Item.get_key_from_object(object), priority)



        // pooled
        this.pooled_array = this.create_pooled_sensation_array(original_event_array, stride=2)
        #this.pooled_array = this.create_pooled_sensation_array(this.pooled_array , stride=2)
        this.pooled_components_bag.clear()

        maximum = 0
        for indices, sentence in np.ndenumerate(this.pooled_array){
            if sentence.value.frequency > WorldConfig.POSITIVE_THRESHOLD \
                    && not (isinstance(sentence.statement,
                                        CompoundTerm) && sentence.statement.connector == NALSyntax.TermConnector.Negation){
                maximum = max(maximum, sentence.value.frequency * sentence.value.confidence)

        for indices, sentence in np.ndenumerate(this.pooled_array){
            if sentence.value.frequency > WorldConfig.POSITIVE_THRESHOLD \
                    && not (isinstance(sentence.statement,
                                        CompoundTerm) && sentence.statement.connector == NALSyntax.TermConnector.Negation){
                priority = sentence.value.frequency * sentence.value.confidence / maximum
                object = indices
                this.pooled_components_bag.PUT_NEW(object)
                this.pooled_components_bag.change_priority(Item.get_key_from_object(object), priority)

    def take(this, pooled){
        /*
            Probabilistically select a spatial subset of the buffer.
            :return: an Array Judgment of the selected subset.
        /
        if pooled:
            bag = this.pooled_components_bag
            array = this.pooled_array
        else:
            bag = this.components_bag
            array = this.array

        // probabilistically peek the 2 vertices of the box
        // selection 1: small fixed windows

        indices = bag.peek()
        if indices == null: return null

        y, x = indices.object
        radius = 1#random.randint(1,2)
        min_x, min_y = max(x - radius, 0), max(y - radius, 0)
        max_x, max_y = min(x + radius, array.shape[1] - 1), min(y + radius,array.shape[0] - 1)

        extracted = array[min_y:max_y+1, min_x:max_x+1]
        sentence_subset= []
        for idx,sentence in np.ndenumerate(extracted){
            if not (isinstance(sentence.statement, CompoundTerm)
                && sentence.statement.connector == NALSyntax.TermConnector.Negation){
                sentence_subset.append(sentence)

        total_truth = null
        statement_subset = []
        for sentence in sentence_subset:
            if total_truth == null:
                total_truth = sentence.value
            else:
                total_truth = this.nars.inferenceEngine.truthValueFunctions.F_Intersection(sentence.value.frequency,
                                                                     sentence.value.confidence,
                                                                     total_truth.frequency,
                                                                     total_truth.confidence)
            statement_subset.append(sentence.statement)


        // create conjunction of features
        statement = CompoundTerm(subterms=statement_subset,
                                                  term_connector=NALSyntax.TermConnector.Conjunction)

        event_sentence = Judgment(statement=statement,
                                      value=total_truth,
                                      occurrence_time=Global.Global.get_current_cycle_number())


        if pooled:
            img_min_x, img_min_y, img_max_x, img_max_y = 2*min_x, 2*min_y, 2*max_x, 2*max_y
        else:
            img_min_x, img_min_y, img_max_x, img_max_y = min_x, min_y, max_x, max_y
        last_taken_img_array = np.zeros(shape=this.img.shape)
        last_taken_img_array[img_min_y+1:(img_max_y+1)+1, img_min_x+1:(img_max_x+1)+1] = this.img[img_min_y+1:(img_max_y+1)+1, img_min_x+1:(img_max_x+1)+1]
        this.last_taken_img_array = last_taken_img_array  // store for visualization

        return event_sentence

    def create_spatial_conjunction(this, subset){
                    /*

                    :param subset: 2d Array of positive (non-negated) sentences / events
                    :return:
                    /
                    conjunction_truth_value = null
                    terms_array = np.empty(shape=subset.shape,
                                           dtype=Term)
                    for indices, sentence in np.ndenumerate(subset){
                        truth_value = sentence.value
                        term = sentence.statement

                        if conjunction_truth_value == null:
                            conjunction_truth_value = truth_value
                        else:
                            conjunction_truth_value = this.nars.inferenceEngine.truthValueFunctions.F_Intersection(conjunction_truth_value.frequency,
                                                           conjunction_truth_value.confidence,
                                                           truth_value.frequency,
                                                           truth_value.confidence)

                        terms_array[indices] = term

                    spatial_conjunction_term = SpatialTerm(spatial_subterms=terms_array,
                                                                            connector=NALSyntax.TermConnector.ArrayConjunction)
                    spatial_conjunction = Judgment(statement=spatial_conjunction_term,
                                                  value=conjunction_truth_value,
                                                  occurrence_time=Global.Global.get_current_cycle_number())

                    return spatial_conjunction

                def create_pooled_sensation_array(this, array, stride){
                    /*
                        Takes an array of events, && returns an array of events except 2x2 pooled with stride
                    :param array:
                    :param stride:
                    :return:
                    //*
                    pad_sentence = Global.Global.ARRAY_NEGATIVE_SENTENCE
        stride_original_sentences = np.empty(shape=(2,2),
                                             dtype=Sentence)
        if stride == 1:
            pool_terms_array = np.empty(shape=(array.shape[0] - 1, array.shape[1] - 1),
                                      dtype=Term)
        else if stride == 2:
            height = array.shape[0] // 2 if array.shape[0] % 2 == 0 else (array.shape[0]+1) // 2
            width = array.shape[1] // 2 if array.shape[1] % 2 == 0 else (array.shape[1]+1) // 2
            pool_terms_array = np.empty(shape=(height, width), dtype=Term)
        else:
            assert false,"ERROR: Does not support stride > 2"

        for indices,sentence in np.ndenumerate(array){
            y, x = indices
            y, x = int(y), int(x)
            if stride == 2 && not (x % 2 == 0 || y % 2 == 0){ continue // only use even indices for stride 2

            pool_y = y // 2 if stride == 2 else y
            pool_x = x // 2 if stride == 2 else x

            // pool sensation
            if y < array.shape[0] - 1 && x < array.shape[1] - 1:
                // not last column || row yet
                stride_original_sentences = np.array(array[y:y+2, x:x+2])  // get 4x4

            else if y == array.shape[0] - 1 && x < array.shape[1] - 1:
                // last row, not last column
                if stride == 1: continue
                stride_original_sentences[0,:] = np.array([array[y, x], array[y, x+1]])
                stride_original_sentences[1,:] = np.array([pad_sentence, pad_sentence])

            else if y < array.shape[0] - 1 && x == array.shape[1] - 1:
                // last column, not last row
                if stride == 1: continue
                stride_original_sentences[0,:] = np.array([array[y, x], pad_sentence])
                stride_original_sentences[1,:] = np.array([array[y+1, x], pad_sentence])

            else if y == array.shape[0] - 1 && x == array.shape[1] - 1:
                if stride == 1: continue
                #last row && column
                stride_original_sentences[0, :] = np.array([array[y, x], pad_sentence])
                stride_original_sentences[1, :] = np.array([pad_sentence, pad_sentence])

            pool_terms_array[pool_y, pool_x] = this.create_spatial_disjunction(np.array(stride_original_sentences))

        return pool_terms_array

    def create_spatial_disjunction(this, array_of_events){
            /*

            :param terms: 2x2 Array of positive (non-negated) sentences / events
            :param terms_array: 2x2 array of potentially negated Terms
            :return:
            /
# TODO FINISH THIS
            all_negative = true
        for i,event in np.ndenumerate(array_of_events){
            all_negative = all_negative \
                           && (isinstance(event.statement, CompoundTerm) && event.statement.connector == NALSyntax.TermConnector.Negation)

        disjunction_truth = null
        disjunction_subterms = np.empty(shape=array_of_events.shape,
                                  dtype=Term)

        for indices, event in np.ndenumerate(array_of_events){
            if isinstance(event.statement, CompoundTerm) \
                    && event.statement.connector == NALSyntax.TermConnector.Negation:
                // negated event, get positive
                truth_value = this.nars.inferenceEngine.truthValueFunctions.F_Negation(event.value.frequency,
                                                                 event.value.confidence) // get regular positive back
                new_statement = event.statement.subterms[0]
            else:
                // already positive
                truth_value = event.value
                new_statement = event.statement

            disjunction_subterms[indices] = new_statement

            if disjunction_truth == null:
                disjunction_truth = truth_value
            else:
                // OR
                disjunction_truth = this.nars.inferenceEngine.truthValueFunctions.F_Union(disjunction_truth.frequency,
                                                                                  disjunction_truth.confidence,
                                                                                  truth_value.frequency,
                                                                                  truth_value.confidence)

        disjunction_term = SpatialTerm(spatial_subterms=disjunction_subterms,
                                                                connector=NALSyntax.TermConnector.ArrayDisjunction)

        if all_negative:
            disjunction_truth = this.nars.inferenceEngine.truthValueFunctions.F_Negation(disjunction_truth.frequency,
                                                                           disjunction_truth.confidence)
            disjunction_term = CompoundTerm(subterms=[disjunction_term],
                                                            term_connector=NALSyntax.TermConnector.Negation)


        spatial_disjunction = Judgment(statement=disjunction_term,
                                      value=disjunction_truth)

        return spatial_disjunction

    def transduce_raw_vision_array(this, img_array){
        /*
            Transduce raw vision data into NARS truth-values
            :param img_array:
            :return: python array of NARS truth values, with the same dimensions as given raw data
        /
        max_value = 255

        def create_2d_truth_value_array(*indices){
            coords = tuple([int(var) for var in indices])
            y,x = coords
            pixel_value = float(img_array[y, x])

            f = pixel_value / max_value
            if f > 1: f = 1

            relative_indices = []
            offsets = (img_array.shape[0]-1)/2, (img_array.shape[1]-1)/2
            for i in range(2){
                relative_indices.append((indices[i] - offsets[i]) / offsets[i])

            unit = HelperFunctions.get_unit_evidence()
            c = unit*math.exp(-1*((WorldConfig.FOCUSY ** 2)*(relative_indices[0]**2) + (WorldConfig.FOCUSX ** 2)*(relative_indices[1]**2)))

            predicate_name = 'B'
            subject_name = str(y) + "_" + str(x)


            if f > WorldConfig.POSITIVE_THRESHOLD:
                truth_value = EvidentialValue(f, c)
                statement = from_string("(" + subject_name + "-->" + predicate_name + ")")
            else:
                truth_value = EvidentialValue(ExtendedBooleanOperators.bnot(f), c)
                statement = from_string("(--,(" + subject_name + "-->" + predicate_name + "))")

            // create the common predicate

            return Judgment(statement=statement,
                                                       value=truth_value)


        func_vectorized = np.vectorize(create_2d_truth_value_array)
        truth_value_array = np.fromfunction(function=func_vectorized,
                                            shape=img_array.shape)

        return truth_value_array;
    }
*/

public class TemporalModule<T>: ItemContainer<T>
{
    /*
        Performs temporal composition
                and
            anticipation (negative evidence for predictive implications)
    */
    NARS nars;
    List<Item<T>> temporal_chain;
    List<T> anticipations_queue;
    T? current_anticipation;

    public TemporalModule(NARS nars, int capacity) : base(capacity) {
        this.nars = nars;
        // temporal chaining
        this.temporal_chain = new List<Item<T>>();

        // anticipation

        this.anticipations_queue = new List<T>();
    }

    public override Item<T> PUT_NEW(T obj) {
        /*
            Put the newest item onto the end of the buffer.

            Returns popped item if buffer overflow, otherwise null;
        */
        Item<T> item = base.PUT_NEW(obj);

        // add to buffer
        this.temporal_chain.Add(item);

        Item<T> popped_item = null;
        // update temporal chain
        if (this.temporal_chain.Count > this.GetCount())
        {
            popped_item = this.temporal_chain[0];
            this.temporal_chain.RemoveAt(0); 
            base._take_from_lookup_dict(popped_item.key);
        }

        return popped_item;

        //this.process_temporal_chaining()
    }


           /*         def process_temporal_chaining(this){
                        if len(this) > 0:
                            this.temporal_chaining_2_conjunction()
                            this.temporal_chaining_2_imp()

                    def get_most_recent_event_task(this){
                        return this.temporal_chain[-1]

                    def temporal_chaining_2_imp(this){
                        *//*
                            Perform temporal chaining

                            produce all possible forward implication statements using temporal induction && intersection
                                A =/> B

                            for the latest statement in the chain
                        *//*
                        NARS = this.NARS
        temporal_chain = this.temporal_chain
        num_of_events = len(temporal_chain)

        event_task_B = this.get_most_recent_event_task().object
        event_B = event_task_B.sentence

        if not isinstance(event_B.statement, StatementTerm){ return #todo remove this. temporarily prevent arrays in postconditions

        def process_sentence(derived_sentence){
            if derived_sentence == not null:
                if NARS == not null:
                    task = Task(derived_sentence)
                    NARS.global_buffer.PUT_NEW(task)

        // produce all possible forward implication statements using temporal induction && intersection
        // A &/ B,
        // A =/> B
        for i in range(0,num_of_events-1){  // && do induction with events occurring afterward
            event_task_A = temporal_chain[i].object
            event_A = event_task_A.sentence

            if not (isinstance(event_A.statement, CompoundTerm)
                    && TermConnectorMethods.is_conjunction(event_A.statement.connector)
                    && isinstance(event_A.statement.subterms[0], CompoundTerm)
                    && TermConnectorMethods.is_conjunction(event_A.statement.subterms[0].connector)){ continue

            derived_sentences = NARSInferenceEngine.do_temporal_inference_two_premise(event_A, event_B)

            for derived_sentence in derived_sentences:
                if not isinstance(derived_sentence.statement, StatementTerm){ continue  // only implications
                process_sentence(derived_sentence)*/

/*    def temporal_chaining_2_conjunction(this){
                        *//*
                            Perform temporal chaining

                            produce all possible forward implication statements using temporal induction && intersection
                                A && B

                            for the latest statement in the chain
                        *//*
                        NARS = this.NARS
        temporal_chain = this.temporal_chain
        num_of_events = len(temporal_chain)

        event_task_B = this.get_most_recent_event_task().object
        event_B = event_task_B.sentence

        if not (isinstance(event_B.statement, CompoundTerm)
                && TermConnectorMethods.is_conjunction(event_B.statement.connector)
                && (isinstance(event_B.statement.subterms[0], SpatialTerm) || isinstance(event_B.statement.subterms[0], StatementTerm))){ return

        def process_sentence(derived_sentence){
            if derived_sentence == not null:
                if NARS == not null:
                    task = Task(derived_sentence)
                    NARS.global_buffer.PUT_NEW(task)

        // A &/ B
        for i in range(0,num_of_events-1){
            event_task_A = temporal_chain[i].object
            event_A = event_task_A.sentence

            if not (isinstance(event_A.statement, CompoundTerm)
                    && TermConnectorMethods.is_conjunction(event_A.statement.connector)
                    && (isinstance(event_A.statement.subterms[0], SpatialTerm) || isinstance(
                        event_A.statement.subterms[0], StatementTerm))){ return

            derived_sentences = NARSInferenceEngine.do_temporal_inference_two_premise(event_A, event_B)

            for derived_sentence in derived_sentences:
                if isinstance(derived_sentence.statement, StatementTerm){ continue  // only conjunctions
                process_sentence(derived_sentence)

    def temporal_chaining_3_conjunction(this){
                                        *//*
                                            Perform temporal chaining

                                            produce all possible forward implication statements using temporal induction && intersection
                                                A && B && C

                                            for the latest statement in the chain
                                        *//*
                                        NARS = this.NARS
        temporal_chain = this.temporal_chain
        num_of_events = len(temporal_chain)

        event_task_C = this.get_most_recent_event_task().object
        event_C = event_task_C.sentence

        if not isinstance(event_C.statement, SpatialTerm){ return

        def process_sentence(derived_sentence){
            if derived_sentence == not null:
                if NARS == not null:
                    task = Task(derived_sentence)
                    NARS.global_buffer.PUT_NEW(task)


        for i in range(0, num_of_events - 1){  // && do induction with events occurring afterward
            event_task_A = temporal_chain[i].object
            event_A = event_task_A.sentence

            if not isinstance(event_A.statement,
                          SpatialTerm) || event_A.statement == event_C.statement: continue

            for j in range(i + 1, num_of_events - 1){
                event_task_B = temporal_chain[j].object
                event_B = event_task_B.sentence

                if not isinstance(event_B.statement, SpatialTerm) \
                    || event_B.statement == event_A.statement \
                    || event_B.statement == event_C.statement: continue

                result_statement = CompoundTerm([event_A.statement,
                                                                  event_B.statement,
                                                                  event_C.statement],
                                                                 NALSyntax.TermConnector.Conjunction)

                truth_value = this.nars.inferenceEngine.truthValueFunctions.F_Intersection(event_A.value.frequency,
                                                                     event_A.value.confidence,
                                                                     event_B.value.frequency,
                                                                     event_B.value.confidence)

                truth_value = this.nars.inferenceEngine.truthValueFunctions.F_Intersection(truth_value.frequency,
                                                                     truth_value.confidence,
                                                                     event_C.value.frequency,
                                                                     event_C.value.confidence)

                truth_value = NALGrammar.Values.EvidentialValue(frequency=truth_value.frequency,
                                                     confidence=truth_value.confidence)
                result = Judgment(statement=result_statement,
                                                       value=truth_value,
                                                       occurrence_time=Global.Global.get_current_cycle_number())

                process_sentence(result)

    def temporal_chaining_3(this){
            *//*
                Perform temporal chaining

                produce all possible forward implication statements using temporal induction && intersection
                    A &/ B,
                    A =/> B
                    &&
                    (A &/ B) =/> C

                for the latest statement in the chain
            *//*

            NARS = this.NARS
        temporal_chain = this.temporal_chain
        num_of_events = len(temporal_chain)

        event_task_C = this.get_most_recent_event_task().object
        event_C = event_task_C.sentence

        def process_sentence(derived_sentence){
            if derived_sentence == not null:
                if NARS == not null:
                    task = Task(derived_sentence)
                    NARS.global_buffer.PUT_NEW(task)

        // produce all possible forward implication statements using temporal induction && intersection
        // A &/ C,
        // A =/> C
        // &&
        // (A &/ B) =/> C

        for i in range(0, num_of_events - 1){  // && do induction with events occurring afterward
            event_task_A = temporal_chain[i].object
            event_A = event_task_A.sentence

            if not isinstance(event_A.statement,
                          SpatialTerm){ continue  // todo remove this eventually. only arrays in precondition

            // produce statements (A =/> C) && (A &/ C)
            if isinstance(event_C.statement,
                              SpatialTerm){
                derived_sentences = NARSInferenceEngine.do_temporal_inference_two_premise(event_A, event_C)

                for derived_sentence in derived_sentences:
                    if isinstance(derived_sentence.statement, StatementTerm){ continue  // ignore simple implications
                    process_sentence(derived_sentence) #todo A_C conjunction only


            for j in range(i + 1, num_of_events - 1){
                event_task_B = temporal_chain[j].object
                event_B = event_task_B.sentence

                conjunction_A_B = Temporal.TemporalIntersection(event_A,
                                                                                  event_B)  // (A &/ B)

                if conjunction_A_B == not null:
                    Sentence derived_sentence = Temporal.TemporalInduction(conjunction_A_B,
                                                                                    event_C)  // (A &/ B) =/> C
                    process_sentence(derived_sentence)


    def temporal_chaining_4(this){
                                    *//*
                                        Perform temporal chaining

                                        produce all possible forward implication statements using temporal induction && intersection
                                            A &/ D,
                                            A =/> D
                                            &&
                                            (A &/ B) =/> D
                                            (A &/ C) =/> D
                                            &&
                                            (A &/ B &/ C) =/> D

                                        for the latest event D in the chain

                                        todo not supported
                                    *//*
                                    NARS = this.NARS
        results = []
        temporal_chain = this.temporal_chain
        num_of_events = len(temporal_chain)

        event_task_D = this.get_most_recent_event_task()
        event_D = event_task_D.sentence

        def process_sentence(derived_sentence){
            if derived_sentence == not null:
                results.append(derived_sentence)
                if NARS == not null:
                    task = Task(derived_sentence)
                    NARS.global_buffer.PUT_NEW(task)

        // produce all possible forward implication statements using temporal induction && intersection
        // A &/ C,
        // A =/> C
        // &&
        // (A &/ B) =/> C
        for i in range(0, num_of_events - 1){  // && do induction with events occurring afterward
            event_task_A = temporal_chain[i].object
            event_A = event_task_A.sentence

            // produce statements (A =/> D) && (A &/ D)
            derived_sentences = NARSInferenceEngine.do_temporal_inference_two_premise(event_A, event_D)

            for derived_sentence in derived_sentences:
                // if isinstance(derived_sentence.statement, StatementTerm){ continue
                process_sentence(derived_sentence)

            for j in range(i + 1, num_of_events - 1){
                event_task_B = temporal_chain[j].object
                event_B = event_task_B.sentence

                conjunction_A_B = Temporal.TemporalIntersection(event_A,
                                                                                  event_B)  // (A &/ B)
                if conjunction_A_B == not null:
                    Sentence derived_sentence = Temporal.TemporalInduction(conjunction_A_B,
                                                                                    event_D)  // (A &/ B) =/> D
                    process_sentence(derived_sentence)

                for k in range(j + 1, num_of_events - 1){
                    if conjunction_A_B == null: break
                    event_task_C = temporal_chain[k].object
                    event_C = event_task_C.sentence
                    conjunction_A_B_C = Temporal.TemporalIntersection(conjunction_A_B,
                                                                                        event_C)  // (A &/ B &/ C)
                    Sentence derived_sentence = Temporal.TemporalInduction(conjunction_A_B_C,
                                                                                    event_D)  // (A &/ B &/ C) =/> D
                    process_sentence(derived_sentence)

        return results

/*    def anticipate_from_event(this, observed_event){
                                                    *//*
                                                        // form new anticipation from observed event
                                                    *//*
                                                    return #todo

        random_prediction = this.NARS.memory.get_random_bag_prediction(observed_event)

        if random_prediction == not null:
            // something == anticipated
            this.anticipate_from_concept(this.NARS.memory.peek_concept(random_prediction.statement),
                                         random_prediction)*/


                    /*    def anticipate_from_concept(this, higher_order_anticipation_concept, best_belief=null){
                                                                        *//*
                                                                            Form an anticipation based on a higher-order concept.
                                                                            Uses the best belief from the belief table, unless one == provided.

                                                                        :param higher_order_anticipation_concept:
                                                                        :param best_belief:
                                                                        :return:
                                                                        *//*
                                                                        return #todo
                            if best_belief == null:
                                best_belief = higher_order_anticipation_concept.belief_table.peek()

                            expectation = best_belief.get_expectation()

                            // use this for 1 anticipation only
                            // if this.current_anticipation == not null:
                            //     // in the middle of a operation sequence already
                            //     current_anticipation_expectation = this.current_anticipation
                            //     if expectation <= current_anticipation_expectation: return // don't execute since the current anticipation == more expected
                            //     // else, the given operation == more expected
                            //     this.anticipations_queue.clear()

                            this.current_anticipation = expectation

                            working_cycles = HelperFunctions.convert_from_interval(
                                higher_order_anticipation_concept.term.interval)

                            postcondition = higher_order_anticipation_concept.term.get_predicate_term()
                            this.anticipations_queue.append([working_cycles, higher_order_anticipation_concept, postcondition])
                            if(this.nars.config.DEBUG) Debug.Log(
                                str(postcondition) + " IS ANTICIPATED FROM " + str(best_belief) + " Total Anticipations:" + str(
                                    len(this.anticipations_queue)))*/

                    /*    def process_anticipations(this){
                                *//*

                                    anticipation (negative evidence for predictive implications)
                                *//*
                                return #todo
                            // process pending anticipations
                            i = 0

                            while i < len(this.anticipations_queue){
                                remaining_cycles, best_prediction_concept, anticipated_postcondition = this.anticipations_queue[
                                    i]  // event we expect to occur
                                anticipated_postcondition_concept = this.NARS.memory.peek_concept(anticipated_postcondition)
                                if remaining_cycles == 0:
                                    if anticipated_postcondition_concept.is_positive(){
                                        // confirmed
                                        if(this.nars.config.DEBUG) Debug.Log(
                                            str(anticipated_postcondition_concept) + " SATISFIED - CONFIRMED ANTICIPATION" + str(
                                                best_prediction_concept.term))
                                    else:
                                        sentence = Judgment(statement=best_prediction_concept.term,
                                                                                 value=NALGrammar.Values.EvidentialValue(frequency=0.0,
                                                                                                                    confidence=WorldConfig.DEFAULT_DISAPPOINT_CONFIDENCE))
                                        if(this.nars.config.DEBUG)
                                            Debug.Log(str(
                                                anticipated_postcondition_concept) + " DISAPPOINT - FAILED ANTICIPATION, NEGATIVE EVIDENCE FOR " + str(
                                                sentence))
                                        this.NARS.global_buffer.PUT_NEW(Task(sentence))
                                    this.anticipations_queue.pop(i)
                                    this.current_anticipation = null
                                    i -= 1
                                else:
                                    this.anticipations_queue[i][0] -= 1

                                i += 1*/
                        }