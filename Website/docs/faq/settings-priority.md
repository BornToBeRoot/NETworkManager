# Setting priority

## In which priority are settings applied?

There are three levels of settings:

- **Global settings**
  Global settings are the default settings of the application. They are used if no other settings are available.

- **Group settings**
  Group settings are applied to all profiles in a group. They overwrite global settings. E.g. for a specific group of servers to overwrite the SSH key file or Windows credentials.

- **Profile settings**
  Profile settings are applied to a specific profile. They overwrite group settings. E.g. for a specific server to overwrite the SSH key file or Windows credentials.

The priority is as follows:

`Global settings (default) → Group settings (overwrites global) → Profile settings (overwrites group)`
