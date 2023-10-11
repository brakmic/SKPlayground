Welcome to SK Playground! 

This project leverages the power of the Microsoft [Semantic Kernel](https://github.com/microsoft/semantic-kernel/tree/main) to interact with the [OpenAI](https://openai.com/) API.

## Companion Article Series
This repository serves as a companion to a series of articles discussing the integration and utilization of Semantic Kernel. These articles provide deeper insights into the concepts and functionalities demonstrated in this repository.

- [Intro to Semantic Kernel - Part One](https://blog.brakmic.com/intro-to-semantic-kernel-part-one/) 
- [Intro to Semantic Kernel - Part Two](https://blog.brakmic.com/intro-to-semantic-kernel-part-two/)
- [Intro to Semantic Kernel - Part Three](https://blog.brakmic.com/intro-to-semantic-kernel-part-three/)

## Project Structure

```plaintext
.
├── SkPlayground.csproj
├── SkPlayground.sln
├── appsettings.plugins.json
├── config
│   └── appsettings.json.example
├── desc
│   ├── action_planner
│   │   ├── create_html_doc.txt
│   │   ├── download_document.txt
│   │   └── find_url.txt
│   ├── dotnet_project.txt
│   ├── keycloak_helm_chart.txt
│   ├── keycloak_prod_with_mysql.txt
│   ├── postgresl_helm_chart.txt
│   ├── sequential_planner
│   │   ├── extract_js.txt
│   │   ├── generate_secret_plan.txt
│   │   └── keycloak_plan.txt
│   └── typescript_nestjs_project.txt
│
├── notebooks
│   └── demo1.ipynb
│
├── scripts
│   └── parse.sh
├── skills
│   ├── DevOps
│   ├── Engineering
│   ├── Html
│   ├── Http
│   ├── KeyAndCertGenerator
│   ├── SecretYamlGenerator
│   └── SecretYamlUpdater
```

## Core Features
SkPlayground is built on C# and [.NET 7](https://dotnet.microsoft.com/en-us/download), using [Semantic Kernel](https://www.nuget.org/packages/Microsoft.SemanticKernel/) from Microsoft. It is equipped with several plugins:

### DevOps Plugin:
- **Kubernetes**: Generates YAML files based on user descriptions to complete specific tasks.
- **Helm**: Creates Helm v3 Charts as per user specifications.

### Engineering Plugin:
- **TypeScript**: Generates a README.md with a detailed description and source codes for building NodeJS projects based on TypeScript.
- **CSharp**: Generates a README.md with a detailed description and source codes for building .NET projects based on C#.

## Html Plugin:
- **CreateHtmlDoc**: Generate a HTML file
- **ExtractJS**: Extract embedded JavaScript from a HTML document.

## Http Plugin:
- **ExecuteGetAsync**: Execute a GET request.
- **ExecutePostAsync**: Execute a POST request.
- **ExecutePutAsync**: Execute a PUT request.

## KeyAndCertGenerator Plugin:
- **GenerateBase64KeyAndCert**: Create a base64-encoded private key and certificate.
- **Extract**: Extract key or certificate from a base64-encoded string.

## SecretYamlGenerator Plugin:
- **CreateSecretYaml**: Create a Kubernetes Secret YAML file.

## SecretYamlUpdater:
- **UpdateKubernetesSecretYamlString**: Update the data section of a Kubernetes Secret YAML.
## Usage Examples

The program can be executed via the command line using the `dotnet run` command, along with specifying two arguments: `-i` (or `--input`) for the input file, and `-f` (or `--function`) for the function to be executed. The input file should contain a description of the task, and the function argument should specify which function to run.

**When using Planners**, there's no need to manually set the function.

**Before executing any task**, ensure to activate the compatible `Run*` method within the *CommandLine* handler.

```cs
 rootCommand.SetHandler(
        //Run,
        //RunWithActionPlanner,
        RunWithSequentialPlanner,
        fileOption, functionOption
    );
```

Here are some examples:

### 1. Generating C# Project README:

This command reads the description from the `dotnet_project.txt` file and executes the `CSharp` function to generate a README for a C# project:

```bash
dotnet run -- -i ./desc/dotnet_project.txt -f CSharp
```

### 2. Generating TypeScript Project README:

Assuming there's a `typescript_project.txt` file with the appropriate description, you can generate a README for a TypeScript project as follows:

```bash
dotnet run -- -i ./desc/typescript_project.txt -f TypeScript
```

### 3. Generating Kubernetes YAML:

If you have a description for a Kubernetes setup in a file called `kubernetes_desc.txt`, you can generate the necessary YAML files using the following command:

```bash
dotnet run -- -i ./desc/kubernetes_desc.txt -f Kubernetes
```

### 4. Generating Helm Charts for Keycloak Deployment:

Given a description in a file named `keycloak_helm_desc.txt`, you can generate Helm v3 Charts for a Keycloak deployment like this:

```bash
dotnet run -- -i ./desc/keycloak_helm_desc.txt -f Helm
```

### Note:
Ensure that the necessary plugins are correctly placed under the "skills" directory and that the descriptions in the input files are well-formatted to get the desired outputs.

## Input Files
Input files are housed in the `desc` folder and contain descriptions provided by the user. Here are a few examples:

### Hashing Application in C# 1:
```plaintext
Create an application that takes a string, hashes it with SHA256, and then returns that hash back to the user.
```

### Deploy a Keycloak Helm Chart 2:
```plaintext
Deploy latest available version of Keycloak (quarkus-based variant) that meets the following criteria:
- uses an external PostgreSQL instance created by another Helm Chart
- runs in production mode
- uses self-signed certificates
- creates a realm named "test-realm"
```

### Deploy Keycloak in Prod Mode to Kubernetes:
```plaintext
Deploy keycloak (quarkus variant) that uses mysql as its backend.
Keycloak runs in prod mode and is TLS secured with a self-signed certificate.
Use images from bitnami.
```

## Skill and Plugin Organization
The functions are organized into either "DevOps" or "Engineering" plugins under the `skills` folder. A plugin is a collection of one or more functions. Each function contains a `config.json` and `skprompt.txt` file for configuration and prompt setup respectively.

## Configuration via `appsettings.plugins.json`

The program's behavior can be tailored using the `appsettings.plugins.json` configuration file. This file is read at the start of the application, allowing you to specify the location of the skills folder as well as the available plugins.

Here's an example configuration:

```json
{
  "SkillSettings": {
    "Root": "skills",
    "Plugins": [
      "DevOps",
      "Engineering",
      "Html",
      "Http",
      "KeyAndCertGenerator",
      "SecretYamlGenerator",
      "SecretYamlUpdater"
    ]
  }
}
```

In this configuration:

- **`Root`**: Specifies the location of the skills folder relative to the project's root directory. By default, it's set to "skills", but you can change it to reference a different folder.
  
- **`Plugins`**: Specifies the available plugins, which are expected to be found within subdirectories of the specified skills folder. In this case, the `DevOps` and `Engineering` plugins are available.

This setup allows a flexible structure, enabling you to organize your skills and plugins as per your project's requirements. You can change the `Root` and `Plugins` settings in the `appsettings.plugins.json` file to point to different directories or to include different sets of plugins, without needing to modify the program's source code.

## Configuration via `appsettings.json`

Additionally, there's a configuration file named `appsettings.json.example` located in the `config` directory. This file is essential for the correct operation of the application, as it contains configurations for the GPT model, service type, and your API key, among other settings.

Here's an example configuration:

```json
{
  "endpointType": "text-completion",
  "serviceType": "OpenAI",
  "serviceId": "text-davinci-003",
  "deploymentOrModelId": "text-davinci-003",
  "apiKey": "... your OpenAI key ...",
  "orgId": ""
}
```

Before running the program, you'll need to:

1. Rename `appsettings.json.example` to `appsettings.json`.
2. Populate the `appsettings.json` file with the correct data:
    - **`endpointType`**: Specifies the endpoint type for the GPT model.
    - **`serviceType`**: Specifies the service type, in this case, `OpenAI`.
    - **`serviceId`** and **`deploymentOrModelId`**: Specify the ID of the GPT model.
    - **`apiKey`**: Your OpenAI API key.
    - **`orgId`**: (Optional) Your organization ID if applicable.

Once these steps are completed, the program will be able to read the `appsettings.json` file and use the specified configurations to interact with the OpenAI GPT model.

Alternatively, instead of placing sensitive information like the API key in the `appsettings.json` file, you can use the `dotnet user-secrets` tool to configure these values securely. This way, the API key and other sensitive data won't be exposed in the `appsettings.json` file. Here's how you can do it:

1. Initialize user secrets for your project:
    ```bash
    dotnet user-secrets init
    ```

2. Set the required secrets:
    ```bash
    dotnet user-secrets set "apiKey" "... your OpenAI key ..."
    ```

Repeat the above command for each configuration setting you'd like to store as a user secret, replacing `"apiKey"` with the configuration key, and `"... your OpenAI key ..."` with the value.

Once these steps are completed, the program will be able to read the configuration values from the user secrets and use the specified configurations to interact with the OpenAI GPT model without exposing sensitive data in the `appsettings.json` file.

## Jupyter Notebook Environment

This project incorporates a Jupyter Notebook environment, allowing for an interactive and dynamic approach to executing and testing code snippets in a live, document-based setting. The `notebooks` directory houses all the Jupyter notebooks related to this project.

### Getting Started with Jupyter Notebooks

1. **Installing Jupyter Notebook Environment**:
    - Ensure you have .NET Interactive installed. Run the following command:
        ```bash
        dotnet tool install --global Microsoft.dotnet-interactive
        ```
    - Install the .NET kernels for Jupyter:
        ```bash
        dotnet interactive jupyter install
        ```
    - Change the current directory to the project root:
        ```bash
        cd path/to/SkPlayground
        ```

2. **Launching Jupyter Notebook**:
    - Launch Jupyter Notebook:
        ```bash
        jupyter notebook
        ```
    This will open the Jupyter Notebook interface in your web browser, where you can navigate to the `notebooks` directory and open the notebook of your choice.


### Working with Libraries and Dependencies in C# Notebooks

In C# Jupyter Notebooks, you can load libraries and dependencies using the `#r "nuget:..."` directive. For instance, to load the `Microsoft.Extensions.Configuration.Json` library, use the following command:
```csharp
#r "nuget:Microsoft.Extensions.Configuration.Json,7.0.0"
```
## Scripts Usage

Inside the `scripts` directory, you will find the script `parse.sh`. This script is designed to process the output generated by the `DevOps` plugin functions `Helm` and `Kubernetes`.

### Helm Function Output Processing:
When processing the output of the `Helm` function, the script helps in generating the Helm charts. It creates the required directories, YAML files, and other necessary items for a Helm chart. Once generated, these Helm charts can be applied using the Helm tool with the following command:

```bash
helm install [CHART] [NAME] --namespace [NAMESPACE]
```
- `[CHART]`: Path to the directory containing the generated Helm chart.
- `[NAME]`: A name you choose for this release of the chart.
- `[NAMESPACE]`: The namespace in which to install the chart. 

### Kubernetes Function Output Processing:
On the other hand, when processing the output of the `Kubernetes` function, the script creates all the necessary YAML files for Kubernetes resources. Once the YAML files are generated, they can be applied to your Kubernetes cluster using the `kubectl` command like so:

```bash
kubectl apply -f [FILENAME]
```
- `[FILENAME]`: The path to the generated YAML file or directory containing the YAML files.
### Practice: Generating a Helm Chart

This project facilitates the creation of Helm charts through a straightforward process. Here is a step-by-step walkthrough of generating a Helm chart using the `Helm` function of the `DevOps` plugin:

1. **Executing the Plugin Function**:
   Begin by running the desired plugin function using `dotnet run`. In this example, we're using the `Helm` function of the `DevOps` plugin to process a description file (`keycloak_helm_chart.txt`). The OpenAI completion result is redirected to a text file (`output.txt`).
   ```bash
   dotnet run -- -i ./desc/keycloak_helm_chart.txt -f Helm > output.txt
   ```

2. **Parsing the Output**:
   Utilize the provided bash script (`parse.sh`) to process the output file, generating the necessary files and directory structure for the Helm chart.
   ```bash
   ./scripts/parse.sh -f output.txt -o keycloak 
   ```

3. **Exploring the Generated Chart**:
   Navigate to the generated chart directory and utilize the `tree` command to visualize the created files and directories. The Helm chart is now ready for use and is structured as per Helm's standard directory structure.
   ```bash
   cd keycloak
   tree -L 2 
   # Output:
   .
   └── keycloak
       ├──  Chart.yaml
       ├──  templates
       └──  values.yaml
   ```
---

This workflow streamlines the process of transforming human-readable descriptions into deployable Helm charts, showcasing the power and efficiency of automating DevOps tasks through the Semantic Kernel and OpenAI's capabilities integrated within this project.

Additionally, this workflow is equally applicable when utilizing the `Kubernetes` function of the `DevOps` plugin. The parsing script (`parse.sh`) is designed to handle both Helm charts and pure Kubernetes YAML outputs seamlessly. When invoked with completions from the `Kubernetes` function, the script generates one or more Kubernetes YAML files instead, following the same directory structuring convention, making it a versatile tool for your DevOps automation tasks.

## Showroom

### Action Planner

![action_planner_debugging](./gifs/action_planner.gif)
### Sequential Planner

![sequential_planner_debugging](./gifs/sequential-planner_with_semantic-kernel.gif)

## Hooks

![sk_hooks](./gifs/sk_hooks.gif)


# Jupyter Notebook

![jupyter_notebook_demo](./gifs/jupyter_notebook_demo.gif)

### LICENSE
[MIT](./LICENSE.md)
