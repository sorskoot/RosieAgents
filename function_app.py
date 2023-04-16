
import azure.functions as func
import logging
from ExtensionLogger import ExtensionLogger
import semantic_kernel as sk
from semantic_kernel.ai.open_ai import AzureTextCompletion

app = func.FunctionApp()

# Learn more at aka.ms/pythonprogrammingmodel

# Get started by running the following code to create a function using a HTTP trigger.


@app.function_name(name="HttpTrigger1")
@app.route(route="hello")
async def test_function(req: func.HttpRequest) -> func.HttpResponse:
    # kernel = sk.create_kernel()

    my_logger = ExtensionLogger(logging)

    kernel = (
        sk.kernel_builder()
        .with_logger(my_logger)
        .build()
    )

    deployment, api_key, endpoint = sk.azure_openai_settings_from_dot_env()

    kernel.config.add_text_backend("dv", AzureTextCompletion(
        deployment, endpoint, api_key, '2022-12-01'))

    skill = kernel.import_semantic_skill_from_directory("./skills", "IdeasSkill")
    
    jamideaSkill = skill["GamejamIdeas"]
    
    context = await jamideaSkill.invoke_async("Time");
        
    return func.HttpResponse(context.result)
    # logging.info('Python HTTP trigger function processed a request.')

    # name = req.params.get('name')
    # if not name:
    #     try:
    #         req_body = req.get_json()
    #     except ValueError:
    #         pass
    #     else:
    #         name = req_body.get('name')

    # if name:
    #     return func.HttpResponse(f"Hello, {name}. This HTTP triggered function executed successfully.")
    # else:
    #     return func.HttpResponse(
    #         "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response.",
    #         status_code=200
    #     )
