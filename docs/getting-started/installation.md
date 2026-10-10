# Getting started with HandyUI

## Early Heads-up

!!! info "Requirements"
    HandyUI is currently **.NET 10** only. 

## Installation

To install HandyUI, you must pick whether you want the core renderer, the Silk\.NET adapter for HandyUI, or the WinForms adapter for HandyUI *(deprecated)*

=== "HandyUI.SilkNet"

    ## Installing the HandyUI adapter for Silk\.NET

    !!! success "You're in the right place."
        If you are looking to easily make modern apps with HandyUI, you are in the correct place. 

    <!--This is the **recommended approach** for making apps with HandyUI. -->

    ### NuGet PMC

    ```NuGet
    NuGet\Install-Package HandyUI.SilkNet
    ```

    ### Developer Command Prompt

    ```batch
    dotnet add package HandyUI.Silknet
    ```

    ### NuGet Package Manager in VS

    1. Go to your solution explorer.
    2. Right click your current project/solution.
    3. Click **"Manage NuGet packages..."**.
    4. Go to the **"Browse"** tab and search for **"HandyUI.SilkNet"**.
    5. Next, select the version you want *(if you don't want a specific version, ignore this step)*
    6. Click **"Install"**.

=== "HandyUI.Core"

    ## Installing the core renderer

    !!! tip "Heads-up"
        This package is not recommended for casual users.

        You will have to **manually wire up** every single thing in the renderer to a windowing framework.

    <!-- You can install the core renderer and all of it's dependecies by just installing HandyUI.Core on NuGet. -->

    ### NuGet PMC

    ```NuGet
    NuGet\Install-Package HandyUI.Core
    ```

    ### Developer Command Prompt

    ```batch
    dotnet add package HandyUI.Core
    ```

    ### NuGet Package Manager in VS

    1. Go to your solution explorer.
    2. Right click your current project/solution.
    3. Click **"Manage NuGet packages..."**.
    4. Go to the **"Browse"** tab and search for **"HandyUI.Core"**.
    5. Next, select the version you want *(if you don't want a specific version, ignore this step)*
    6. Click **"Install"**.

=== "HandyUI.WinForms"

    ## Installing the HandyUI adapter for WinForms

    !!! danger "Important Heads-up"
        This package is deprecated and is no longer being updated.

    <!-- This is **not recommended** for making apps with HandyUI as the HandyUI adapter for WinForms is **deprecated** and is **not being updated**. -->

    ### NuGet PMC

    ```NuGet
    NuGet\Install-Package HandyUI.WinForms
    ```

    ### Developer Command Prompt

    ```batch
    dotnet add package HandyUI.WinForms
    ```

    ### NuGet Package Manager in VS

    1. Go to your solution explorer.
    2. Right click your current project/solution.
    3. Click **"Manage NuGet packages..."**.
    4. Go to the **"Browse"** tab and search for **"HandyUI.WinForms"**.
    5. Next, select the version you want *(if you don't want a specific version, ignore this step)*
    6. Click **"Install"**.