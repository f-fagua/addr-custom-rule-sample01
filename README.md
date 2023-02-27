# Addressables Assets Changed Custom Rule
## Problem Statement
Users constantly find themselves with bundles with different hashes. Most of the time, they are unaware of what has changed due to unintended changes. With the current UI and inspection tools, it is hard to track what has changed in specific. 
## Description
This repo has a custom rule that uses a scriptable object to compare which asset dependency hashes have changed. The tool subscribes itself to an SBP build callback and gets the addressable entries hashes at the time being. Later when the rule is run, it compares the stored hash with the current one.
