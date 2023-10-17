---
layout: default
title: Bit Calculator
parent: Application
grand_parent: Documentation
nav_order: 22
description: "Documentation of the Bit Calculator"
permalink: /Documentation/Application/BitCalculator
---

# Bit Calculator

New Feature
{: .label .label-green }

2022.12.20.0
{: .label .label-purple }

With the **Bit Calculator** different data units can be converted. Based on the input number and the selected unit the different units like bits, bytes, kilobits, kilobytes, megabits, megabytes, etc. are calculated.

![BitCalculator](21_BitCalculator.png)

{: .note}
Right-click on the result to copy the information.

<hr>

## Settings

### Notation

Notation which should be used for the calculation.

**Type:** `NETworkManager.Models.Network.BitCaluclatorNotation`

**Default:** `Binary`

**Possible values:**

- `Binary` (1024)
- `Decimal` (1000)
