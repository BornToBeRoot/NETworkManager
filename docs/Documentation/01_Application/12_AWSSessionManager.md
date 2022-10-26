---
layout: default
title: AWS Session Manager
parent: Application
grand_parent: Documentation
nav_order: 12
description: "Documentation of the AWS Session Manager"
permalink: /Documentation/Application/AWSSessionManager
---

# AWS Session Manager

With AWS (Systems Manager) Session Manager, you can connect to and manage an EC2 instance without opening inbound ports, running a bastion host, or managing SSH keys. Here you can find more information about [AWS Systems Manager](https://aws.amazon.com/systems-manager/){:target="\_blank"} and the documentation for [AWS Systems Manager Session Manager](https://docs.aws.amazon.com/systems-manager/latest/userguide/session-manager.html){:target="\_blank"}.

This feature allows you to use the `aws ssm start-session --target <INSTANCE_ID>` command with tabs. You can create profiles for your instances or synchronize them from AWS EC2 to connect to them directly.

![AWSSessionManager](12_AWSSessionManager.png)

## Prerequisites

The following prerequisites must be met to use AWS Systems Manager Session Manager.

1. [Setup AWS CLI & Session Manager plugin](#setup-aws-cli--session-manager-plugin)
2. [Setup AWS Systems Manager Session Manager](#setup-aws-systems-manager-session-manager)
3. [Setup AWS IAM user to sync and connect](#setup-aws-iam-user-to-sync-and-connect)

### Setup AWS CLI & Session Manager plugin

The AWS CLI and AWS Session Manager plugin is required on **your computer** to run the `aws ssm start-session` command. You can download them here:

- [AWS CLI](https://aws.amazon.com/cli/){:target="\_blank"} (direct link [Windows installer](https://awscli.amazonaws.com/AWSCLIV2.msi){:target="\_blank"})
- [AWS Session Manager plugin](https://docs.aws.amazon.com/systems-manager/latest/userguide/session-manager-working-with-install-plugin.html){:target="\_blank"} (direct link [Windows installer](https://s3.amazonaws.com/session-manager-downloads/plugin/latest/windows/SessionManagerPluginSetup.exe){:target="\_blank"})

{: .note }
See the AWS documentation for installation instructions.

### Setup AWS Systems Manager Session Manager

To connect to the instances, the AWS Systems Manager Session Manager must be configured in AWS. See their documentation for instructions on [how to set up the Session Manager](https://docs.aws.amazon.com/systems-manager/latest/userguide/session-manager-getting-started.html).

```json
{
  "schemaVersion": "1.0",
  "description": "Document to hold regional settings for Session Manager",
  "sessionType": "Standard_Stream",
  "inputs": {
    "s3BucketName": "<S3_BUCKET>",
    "s3KeyPrefix": "<S3_BUCKET_PREFIX>",
    "s3EncryptionEnabled": true,
    "cloudWatchLogGroupName": "<CLOUDWATCH_GROUPNAME>",
    "cloudWatchEncryptionEnabled": true,
    "cloudWatchStreamingEnabled": false,
    "kmsKeyId": "<KMS_KEY_ARN>",
    "runAsEnabled": true,
    "runAsDefaultUser": "<SSM_RUNASUSER>",
    "idleSessionTimeout": "20",
    "maxSessionDuration": "60",
    "shellProfile": {
      "windows": "<LINUX_COMMANDS>",
      "linux": "<WINDOWS_COMMANDS>"
    }
  }
}
```

{: .note }
This is an example and may not be suitable for a production environment.

```bash
aws ssm create-document \
    --name SSM-SessionManagerRunShell \
    --content "file://SessionManagerRunShell.json" \
    --document-type "Session" \
    --document-format JSON
```

IAM role / Instance profile

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ec2.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
```

Instance policy

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ssmmessages:CreateControlChannel",
        "ssmmessages:CreateDataChannel",
        "ssmmessages:OpenControlChannel",
        "ssmmessages:OpenDataChannel",
        "ssm:UpdateInstanceInformation"
      ],
      "Resource": "*"
    } /*,
        {
            "Effect": "Allow",
            "Action": [
                "logs:CreateLogStream",
                "logs:PutLogEvents",
                "logs:DescribeLogGroups",
                "logs:DescribeLogStreams"
            ],
            "Resource": "*"
        },
        {
            "Effect": "Allow",
            "Action": [
                "s3:PutObject"
            ],
            "Resource": "arn:aws:s3:::<S3_BUCKET>/<S3_BUCKET_PREFIX>/*"
        },
        {
            "Effect": "Allow",
            "Action": [
                "s3:GetEncryptionConfiguration"
            ],
            "Resource": "*"
        },
        {
            "Effect": "Allow",
            "Action": [
                "kms:Decrypt"
            ],
            "Resource": "<KMS_KEY_ARN>"
        },
        {
            "Effect": "Allow",
            "Action": "kms:GenerateDataKey",
            "Resource": "*"
        }*/
  ]
}
```

{: .note }
This is an example and may not be suitable for a production environment.

### Setup AWS IAM user to sync and connect

Sync policy

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "AllowNETworkManagerSync",
      "Effect": "Allow",
      "Action": ["ec2:DescribeInstances", "ec2:DescribeInstanceStatus"],
      "Resource": "*"
    }
  ]
}
```

Connect policy

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["ssm:StartSession"],
      "Resource": [
        "arn:aws:ec2:<AWS_REGION>:<ACCOUNT_ID>:instance/*",
        "arn:aws:ssm:<AWS_REGION>:<ACCOUNT_ID>:document/SSM-SessionManagerRunShell"
      ]
    },
    {
      "Effect": "Allow",
      "Action": [
        "ssm:DescribeSessions",
        "ssm:GetConnectionStatus",
        "ssm:DescribeInstanceProperties",
        "ec2:DescribeInstances"
      ],
      "Resource": "*"
    },
    {
      "Effect": "Allow",
      "Action": ["ssm:TerminateSession", "ssm:ResumeSession"],
      "Resource": ["arn:aws:ssm:*:*:session/${aws:username}-*"]
    } /*,
    {
      "Effect": "Allow",
      "Action": ["kms:GenerateDataKey"],
      "Resource": "<KMS_KEY_ARN>"
    }*/
  ]
}
```

{: .note }
This is an example and may not be suitable for a production environment.

## Connect

### Instance ID

ID of the AWS EC2 instance.

**Type:** `String`

### Profile

AWS CLI profile which will be used to connect.

**Type:** `String`

{: .note }
If not set, the AWS CLI default settings are used!

### Region

AWS region where the instance is located.

**Type:** `String`

{: .note }
If not set, the AWS CLI default settings are used!

## Profile

### Instance ID

ID of the AWS EC2 instance.

**Type:** `String`

### Profile

AWS CLI profile which will be used to connect.

**Type:** `String`

{: .note }
If not set, the [`Default profile`](#default-profile) from the settings is used!

### Region

AWS region where the instance is located.

**Type:** `String`

{: .note }
If not set, the [`Default region`](#default-region) from the settings is used!

## Settings

### Synchronize EC2 instances from AWS

If enabled, EC2 instances are synced from AWS. In addition, the [profiles and regions](#profiles-and-regions-to-synchronize) to be synchronized must be configured.

**Type:** `Boolean`

**Default:** `Disabled`

### Profiles and regions to synchronize

Here you can specify a combination of AWS CLI profile and AWS region from where the EC2 instances should be synchronized. Multiple AWS accounts and regions are supported.

**Type:** `List<NETworkManager.Models.AWS.AWSProfileInfo>`

| Property  | Type      |
| --------- | --------- |
| `Enabled` | `Boolean` |
| `Profile` | `String`  |
| `Region`  | `String`  |

**Default:**

| Enabled    | Profile   | Region         |
| ---------- | --------- | -------------- |
| `Disabled` | `default` | `eu-central-1` |
| `Disabled` | `default` | `us-east-1`    |

**Example:**

| Enabled    | Profile | Region         |
| ---------- | ------- | -------------- |
| `Disabled` | `dev`   | `eu-central-1` |
| `Disabled` | `dev`   | `us-east-1`    |
| `Disabled` | `prod`  | `eu-central-1` |
| `Disabled` | `prod`  | `us-east-1`    |

{: .note }
Only enabled profiles are synchronized and [`Synchronize EC2 instances from AWS`](#synchronize-ec2-instances-from-aws) must be enabled!

### Synchronize only running EC2 instances from AWS

If enabled, only EC2 instances are synchronized where the `instance state` is `running`.

**Type:** `Boolean`

**Default:** `Enabled`

### Default profile

**Type:** `String`

AWS CLI profile which will be used to connect.

**Type:** `String`

{: .note }
If not set, the AWS CLI default settings are used!

### Default region

AWS region where the instance is located.

**Type:** `String`

{: .note }
If not set, the AWS CLI default settings are used!

### PowerShell

Path to the PowerShell console where the AWS CLI is available and which should be embedded in the program.

**Type:** `String`

**Default:** `C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe`

**Possible values:**

- `path\to\PowerShell.exe`
- `path\to\pwsh.exe`
