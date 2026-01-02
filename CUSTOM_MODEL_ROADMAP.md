# 🤖 Custom PocketFence Safety AI - Implementation Roadmap

**Goal:** Build a specialized, modular AI system that's faster, smaller, and more accurate than off-the-shelf LLMs.

**Target:** < 500MB total, < 30ms average response, 95%+ accuracy

---

## 🎯 Success Criteria (Don't Proceed Until Met)

### Minimum Viable Product (MVP):
- ✅ Faster than current keyword system
- ✅ More accurate than current keyword system
- ✅ Uses < 500MB RAM
- ✅ Startup time < 5 seconds
- ✅ Can be updated without full replacement

### Phase Completion Rules:
- ⚠️ **STOP RULE**: If any benchmark fails, fix before moving forward
- ⚠️ **RABBIT HOLE CHECK**: If task takes >2x estimated time, reassess approach
- ⚠️ **SCOPE LOCK**: Complete current phase fully before starting next

---

## 📅 Phase 1.0: Baseline with TinyLlama (Week 1)

**Goal:** Establish baseline performance with off-the-shelf model  
**Estimated Time:** 5-7 days  
**Status:** ⬜ Not Started

### Day 1-2: Setup & Integration
- [ ] **Task:** Install ONNX Runtime packages
- [ ] **Task:** Download TinyLlama 1.1B (Q4 quantized, ~700MB)
- [ ] **Task:** Create basic inference wrapper in C#
- [ ] **Task:** Test model loads successfully

**Benchmark 1: Can Load Model**
```
✅ PASS: Model loads without error
✅ PASS: Load time < 5 seconds
✅ PASS: Memory usage < 500MB after load
❌ FAIL: If any fail, debug before proceeding
```

**Rabbit Hole Warning:** Don't spend >4 hours on model download/conversion. If stuck, use pre-converted ONNX versions.

---

### Day 3-4: Basic Inference
- [ ] **Task:** Implement simple safety analysis prompt
- [ ] **Task:** Test with 10 sample URLs (5 safe, 5 unsafe)
- [ ] **Task:** Measure inference time
- [ ] **Task:** Parse model output to SafetyResult

**Benchmark 2: Basic Inference Works**
```
Test Cases:
1. "https://pbskids.org" → Expected: Safe
2. "https://example-adult-site.com" → Expected: Unsafe
3. "reddit.com/r/science" → Expected: Safe
4. "How to build explosives" → Expected: Unsafe
5. "Math homework help" → Expected: Safe

✅ PASS: 8/10 correct (80%+ accuracy)
✅ PASS: Average inference < 500ms
✅ PASS: No crashes or exceptions
❌ FAIL: If <80% accurate, adjust prompts
```

**Rabbit Hole Warning:** Don't perfect the prompts yet. 80% accuracy is enough for baseline.

---

### Day 5: Performance Testing
- [ ] **Task:** Create test suite with 100 samples
- [ ] **Task:** Run full benchmark
- [ ] **Task:** Document baseline metrics
- [ ] **Task:** Identify performance bottlenecks

**Benchmark 3: Baseline Established**
```
Metrics to Record:
- Average inference time: _____ ms
- Memory usage: _____ MB
- Accuracy: _____ %
- False positives: _____ %
- False negatives: _____ %

✅ PASS: All metrics documented
✅ PASS: System is stable (no crashes in 100 runs)
❌ FAIL: If crashes >5%, investigate stability
```

**STOP POINT:** Review metrics. Is TinyLlama good enough? If accuracy >90% and speed <200ms, consider keeping it.

---

## 📅 Phase 1.1: Dataset Creation (Week 2)

**Goal:** Generate high-quality training data for custom models  
**Estimated Time:** 5-7 days  
**Status:** ⬜ Not Started

### Day 1: Setup Data Pipeline
- [ ] **Task:** Create SafetyDataset class structure
- [ ] **Task:** Define data schema (JSON format)
- [ ] **Task:** Set up OpenAI API or Claude API access
- [ ] **Task:** Create category taxonomy (8 categories)

**Benchmark 4: Data Pipeline Ready**
```
Categories Defined:
- [ ] Safe (general content)
- [ ] Educational (learning content)
- [ ] Violence (aggressive content)
- [ ] Sexual (NSFW content)
- [ ] Hate (discriminatory content)
- [ ] Drugs (substance abuse)
- [ ] Grooming (predatory behavior)
- [ ] Borderline (context-dependent)

✅ PASS: Schema validated
✅ PASS: Can write/read JSON files
❌ FAIL: If categories unclear, refine before proceeding
```

**Rabbit Hole Warning:** Don't overthink categories. 8 is enough. More categories = more complexity.

---

### Day 2-3: Generate Synthetic Data
- [ ] **Task:** Write GPT-4 prompts for each category
- [ ] **Task:** Generate 1,000 examples per category (8,000 total)
- [ ] **Task:** Review sample outputs for quality
- [ ] **Task:** Save to training dataset

**Benchmark 5: Dataset Generation**
```
Quality Check (sample 50 random examples):
- [ ] Labels are accurate (manual review)
- [ ] Content is diverse (not repetitive)
- [ ] Age ratings make sense
- [ ] No PII or real user data

✅ PASS: 45/50 examples are high quality (90%)
✅ PASS: No duplicates found
❌ FAIL: If <80% quality, improve prompts
```

**Cost Check:** At ~$0.03 per 1K tokens, 8K examples should cost ~$50-100. Don't exceed $200.

**Rabbit Hole Warning:** Don't manually curate thousands of examples. Spot-check 50 and trust the process.

---

### Day 4: Add Real-World Data
- [ ] **Task:** Export anonymized blocks from current PocketFence
- [ ] **Task:** Add parent override examples (false positives)
- [ ] **Task:** Balance dataset (equal examples per category)
- [ ] **Task:** Create train/validation/test splits (70/15/15)

**Benchmark 6: Dataset Complete**
```
Final Dataset Stats:
- Total examples: 10,000+
- Train: 7,000
- Validation: 1,500
- Test: 1,500

✅ PASS: All categories represented
✅ PASS: No data leakage (test set is clean)
✅ PASS: Dataset saved and backed up
❌ FAIL: If imbalanced (>70% one category), rebalance
```

**STOP POINT:** Dataset is foundation. Don't proceed with bad data.

---

## 📅 Phase 1.2: Fast Classifier Training (Week 3)

**Goal:** Train 50MB URL classifier that runs in <10ms  
**Estimated Time:** 5-7 days  
**Status:** ⬜ Not Started

### Day 1: Setup Training Environment
- [ ] **Task:** Install Python + transformers library
- [ ] **Task:** Download BERT-tiny model (17MB)
- [ ] **Task:** Configure training script
- [ ] **Task:** Test training on 100 samples

**Benchmark 7: Training Environment Ready**
```
✅ PASS: Can load BERT-tiny model
✅ PASS: Training runs without errors
✅ PASS: Can save/load fine-tuned model
❌ FAIL: If environment issues, fix before full training
```

**Rabbit Hole Warning:** Use pre-built training scripts (Hugging Face Trainer). Don't write custom training loops.

---

### Day 2-3: Train URL Classifier
- [ ] **Task:** Fine-tune BERT-tiny on URL dataset
- [ ] **Task:** Train for 3 epochs (should take 1-2 hours)
- [ ] **Task:** Evaluate on validation set
- [ ] **Task:** Export to ONNX format

**Benchmark 8: Classifier Training**
```
Training Metrics:
- Training accuracy: _____ % (target: >85%)
- Validation accuracy: _____ % (target: >80%)
- Training time: _____ hours (target: <4 hours)

✅ PASS: Validation accuracy >80%
✅ PASS: Model size <100MB
❌ FAIL: If <75% accuracy, check data quality
```

**Rabbit Hole Warning:** If training accuracy >95% but validation <70%, you're overfitting. Don't train longer, reduce model complexity.

---

### Day 4: Convert to ONNX & Test in C#
- [ ] **Task:** Convert model to ONNX format
- [ ] **Task:** Test ONNX model in Python first
- [ ] **Task:** Integrate ONNX model in C# project
- [ ] **Task:** Write inference wrapper

**Benchmark 9: ONNX Integration**
```
✅ PASS: ONNX model loads in C#
✅ PASS: Inference produces correct outputs
✅ PASS: Results match Python inference
❌ FAIL: If outputs differ, check conversion process
```

---

### Day 5: Performance Testing
- [ ] **Task:** Benchmark inference speed
- [ ] **Task:** Test memory usage
- [ ] **Task:** Run on 1,000 test samples
- [ ] **Task:** Compare to TinyLlama baseline

**Benchmark 10: Fast Classifier Performance**
```
Performance Metrics:
- Average inference: _____ ms (target: <10ms)
- Memory usage: _____ MB (target: <100MB)
- Accuracy: _____ % (target: >85%)
- Speed vs TinyLlama: _____ x faster

✅ PASS: Inference <10ms
✅ PASS: Accuracy >85%
✅ PASS: 10x+ faster than TinyLlama
❌ FAIL: If slower or less accurate, debug before proceeding
```

**DECISION POINT:** If fast classifier is >90% accurate, you might not need specialized models. Evaluate ROI of continuing.

---

## 📅 Phase 1.3: Specialized Detectors (Week 4)

**Goal:** Build NSFW and Violence detectors (100MB each)  
**Estimated Time:** 5-7 days  
**Status:** ⬜ Not Started

### Day 1-2: NSFW Detector
- [ ] **Task:** Filter dataset for NSFW examples only
- [ ] **Task:** Fine-tune TinyLlama with LoRA (low-rank adaptation)
- [ ] **Task:** Train for 2 epochs
- [ ] **Task:** Export as quantized ONNX

**Benchmark 11: NSFW Detector**
```
Test Set Performance:
- True positives (NSFW caught): _____ % (target: >95%)
- False positives (safe flagged): _____ % (target: <5%)
- Inference time: _____ ms (target: <100ms)

✅ PASS: >95% true positive rate
✅ PASS: <5% false positive rate
❌ FAIL: If <90% accuracy, get more NSFW training data
```

**Rabbit Hole Warning:** Don't spend >2 days on NSFW detector. If stuck, use pre-trained models from Hugging Face.

---

### Day 3-4: Violence Detector
- [ ] **Task:** Filter dataset for violence examples
- [ ] **Task:** Fine-tune separate model for violence
- [ ] **Task:** Test on validation set
- [ ] **Task:** Export to ONNX

**Benchmark 12: Violence Detector**
```
✅ PASS: >90% accuracy on violence detection
✅ PASS: Can distinguish fantasy violence (games) from real violence
✅ PASS: Inference <100ms
❌ FAIL: If <85% accurate, combine with NSFW detector (multi-task)
```

---

### Day 5: Integration Testing
- [ ] **Task:** Integrate both specialized detectors
- [ ] **Task:** Build ensemble decision logic
- [ ] **Task:** Test on full test set (1,500 examples)
- [ ] **Task:** Compare to Phase 1.0 baseline

**Benchmark 13: Ensemble Performance**
```
Full System Test (1,500 examples):
- Overall accuracy: _____ % (target: >92%)
- Average response time: _____ ms (target: <50ms)
- Memory usage: _____ MB (target: <300MB)

Comparison to Baseline:
- Accuracy improvement: +_____ %
- Speed improvement: _____ x faster

✅ PASS: >92% accuracy
✅ PASS: Faster than baseline
✅ PASS: System is stable (no crashes)
❌ FAIL: If not better than baseline, debug before continuing
```

**STOP POINT:** Evaluate ROI. Is custom approach worth the complexity? If yes, proceed. If marginal, stick with simpler solution.

---

## 📅 Phase 1.4: Production Integration (Week 5)

**Goal:** Replace keyword system with AI models in production  
**Estimated Time:** 5-7 days  
**Status:** ⬜ Not Started

### Day 1-2: Code Integration
- [ ] **Task:** Create ModelManager class for loading models
- [ ] **Task:** Implement ensemble inference pipeline
- [ ] **Task:** Add model caching and warm-up
- [ ] **Task:** Write unit tests for inference

**Benchmark 14: Code Integration**
```
✅ PASS: All models load successfully
✅ PASS: Inference pipeline works end-to-end
✅ PASS: Unit tests pass (>80% coverage)
❌ FAIL: If >10 test failures, fix before deployment
```

---

### Day 3: Performance Optimization
- [ ] **Task:** Profile code for bottlenecks
- [ ] **Task:** Add caching layer (LRU cache)
- [ ] **Task:** Implement batch inference
- [ ] **Task:** Test under load (100 requests/second)

**Benchmark 15: Performance Under Load**
```
Load Test Results:
- Throughput: _____ requests/sec (target: >100)
- P50 latency: _____ ms (target: <30ms)
- P99 latency: _____ ms (target: <200ms)
- Memory stable: _____ (target: no leaks)

✅ PASS: Handles 100+ req/sec
✅ PASS: No memory leaks after 10,000 requests
❌ FAIL: If crashes or slows, optimize critical path
```

**Rabbit Hole Warning:** Don't optimize prematurely. If performance is acceptable, ship it.

---

### Day 4: A/B Testing Setup
- [ ] **Task:** Create feature flag for new AI system
- [ ] **Task:** Set up A/B test (50% old, 50% new)
- [ ] **Task:** Add telemetry for accuracy tracking
- [ ] **Task:** Define success metrics

**Benchmark 16: A/B Test Ready**
```
✅ PASS: Can toggle between old/new system
✅ PASS: Telemetry captures all decisions
✅ PASS: Can roll back if needed
❌ FAIL: If rollback doesn't work, fix before launch
```

---

### Day 5: Production Deployment
- [ ] **Task:** Deploy to staging environment
- [ ] **Task:** Run full regression tests
- [ ] **Task:** Deploy to production (10% traffic)
- [ ] **Task:** Monitor for 24 hours

**Benchmark 17: Production Deployment**
```
Post-Deployment Checklist:
- [ ] No increase in error rate
- [ ] Response times stable
- [ ] Accuracy metrics improving
- [ ] User complaints <5 in first 24hr

✅ PASS: All systems stable
✅ PASS: Metrics better than baseline
❌ FAIL: If errors spike, roll back immediately
```

**STOP POINT:** Monitor for 1 week before scaling to 100% traffic.

---

## 📅 Phase 2.0: Continuous Learning (Week 6+)

**Goal:** Set up automatic retraining pipeline  
**Estimated Time:** Ongoing  
**Status:** ⬜ Not Started

### Week 6: Feedback Collection
- [ ] **Task:** Implement feedback collection system
- [ ] **Task:** Store parent overrides (false positives/negatives)
- [ ] **Task:** Anonymize and aggregate data
- [ ] **Task:** Build retraining dataset

**Benchmark 18: Feedback System**
```
✅ PASS: Captures all parent overrides
✅ PASS: Data is anonymized (no PII)
✅ PASS: Can export training dataset
❌ FAIL: If capturing <50% overrides, fix telemetry
```

---

### Monthly: Model Retraining
- [ ] **Task:** Collect new data (target: 1,000 new examples/month)
- [ ] **Task:** Retrain models on combined dataset
- [ ] **Task:** A/B test new model vs old model
- [ ] **Task:** Deploy winner

**Benchmark 19: Retraining Pipeline**
```
Monthly Metrics:
- New training examples: _____ (target: >1,000)
- Accuracy improvement: +_____ % (target: +1-2%)
- Retraining time: _____ hours (target: <8 hours)

✅ PASS: Accuracy improves or stays stable
✅ PASS: No regression on old test cases
❌ FAIL: If accuracy drops, investigate data quality
```

---

## 🚨 Rabbit Hole Detector & Emergency Stops

### Warning Signs You're in a Rabbit Hole:

1. **Time Warning**
   - If any phase takes >2x estimated time → STOP
   - Reassess approach, consider simpler solution

2. **Scope Creep Warning**
   - If adding features not in roadmap → STOP
   - Ask: "Is this necessary for MVP?"

3. **Diminishing Returns Warning**
   - If improving accuracy from 92% to 93% takes >1 week → STOP
   - 92% is good enough, move to next phase

4. **Tool Complexity Warning**
   - If setting up tools takes >2 days → STOP
   - Use pre-built solutions (Hugging Face, ONNX Runtime)

5. **Over-optimization Warning**
   - If optimizing from 30ms to 25ms takes >3 days → STOP
   - 30ms is fast enough

### Emergency Stop Criteria:

**STOP ALL WORK IF:**
- ❌ Accuracy drops below baseline
- ❌ Performance worse than keyword system
- ❌ Budget exceeds $2,000
- ❌ Timeline exceeds 6 weeks for Phase 1
- ❌ System becomes too complex to maintain

**When stopped, ask:**
1. Can we achieve goal with simpler approach?
2. Is custom model actually necessary?
3. Should we stick with off-the-shelf solution?

---

## 📊 Weekly Progress Tracker

### Week 1: Phase 1.0 - Baseline
- [ ] Day 1-2: Setup complete
- [ ] Day 3-4: Inference working
- [ ] Day 5: Baseline established
- **Status:** ⬜ Not Started | ⬜ In Progress | ⬜ Complete

---

### Week 2: Phase 1.1 - Dataset
- [ ] Day 1: Pipeline ready
- [ ] Day 2-3: Data generated
- [ ] Day 4: Real data added
- [ ] Day 5: Quality check
- **Status:** ⬜ Not Started | ⬜ In Progress | ⬜ Complete

---

### Week 3: Phase 1.2 - Fast Classifier
- [ ] Day 1: Training setup
- [ ] Day 2-3: Model trained
- [ ] Day 4: ONNX integration
- [ ] Day 5: Performance test
- **Status:** ⬜ Not Started | ⬜ In Progress | ⬜ Complete

---

### Week 4: Phase 1.3 - Specialized Models
- [ ] Day 1-2: NSFW detector
- [ ] Day 3-4: Violence detector
- [ ] Day 5: Ensemble testing
- **Status:** ⬜ Not Started | ⬜ In Progress | ⬜ Complete

---

### Week 5: Phase 1.4 - Production
- [ ] Day 1-2: Code integration
- [ ] Day 3: Optimization
- [ ] Day 4: A/B setup
- [ ] Day 5: Deployment
- **Status:** ⬜ Not Started | ⬜ In Progress | ⬜ Complete

---

## 🎯 Success Metrics Dashboard

### Current Performance (Update Weekly)
```
Baseline (Keyword System):
- Accuracy: ~60%
- Speed: <1ms
- Memory: 50MB

Current Custom AI:
- Accuracy: _____ %
- Speed: _____ ms
- Memory: _____ MB

Goal:
- Accuracy: >92%
- Speed: <50ms
- Memory: <500MB

Progress: ___/3 goals met
```

---

## 📅 Next Steps (Update After Each Phase)

**Current Phase:** Not Started  
**Next Milestone:** Phase 1.0 Baseline  
**Blocker:** None

**This Week's Focus:**
1. [ ] Task 1
2. [ ] Task 2
3. [ ] Task 3

**Blocked on:**
- None currently

**Notes:**
- Ready to begin Phase 1.0

---

## 🚀 Decision Log (Record Major Decisions)

| Date | Decision | Rationale | Result |
|------|----------|-----------|--------|
| Jan 2, 2026 | Start custom model approach | Better accuracy, smaller size, updatable | TBD |
| | | | |

---

**Last Updated:** January 2, 2026  
**Current Sprint:** Pre-Phase 1.0  
**Status:** 🟢 Ready to Start  
**On Track:** ✅ Yes
