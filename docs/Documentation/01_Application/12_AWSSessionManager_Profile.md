---
layout: default
title: Profile
parent: AWS Session Manager
grand_parent: Documentation
nav_order: 12
description: ""
permalink: /Documentation/Application/AWSSessionManager-Profile
---

# Profile

# Instance ID

ID of the AWS EC2 instance.

**Type:** `String`

# Profile

AWS CLI profile which will be used to connect.

**Type:** `String`

{: .note }
If not set, the [`Default profile`](#default-profile) from the settings is used!

# Region

AWS region where the instance is located.

**Type:** `String`

{: .note }
If not set, the [`Default region`](#default-region) from the settings is used!
