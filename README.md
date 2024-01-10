# HackerNewsAPI

I was given a task as part of an interview process for a c# developer at a reputable bank.
The task is 
Using ASP.NET Core, implement a RESTful API to retrieve the details of the best n stories from the Hacker News API, as determined by their score, where n is
specified by the caller to the API.
The Hacker News API is documented here: https://github.com/HackerNews/API .
The IDs for the stories can be retrieved from this URI: https://hacker-news.firebaseio.com/v0/beststories.json .
The details for an individual story ID can be retrieved from this URI: https://hacker-news.firebaseio.com/v0/item/21233041.json (in this case for the story with ID
21233041 )
The API should return an array of the best n stories as returned by the Hacker News API in descending order of score, in the form:
[
{
"title": "A uBlock Origin update was rejected from the Chrome Web Store",
"uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
"postedBy": "ismaildonmez",
"time": "2019-10-12T13:43:01+00:00",
"score": 1716,
"commentCount": 572
},
{ ... },
{ ... },
{ ... },
...
]
In addition to the above, your API should be able to efficiently service large numbers of requests without risking overloading of the Hacker News API.
You should share a public repository with us, that should include a README.md file which describes how to run the application, any assumptions you have made, and
any enhancements or changes you would make, given the time


## Setup and Running the Application

### Prerequisites
- https://dotnet.microsoft.com/en-us/download/dotnet/8.0 installed
- developed using visual studio 2022

### Getting Started
1. Clone this repository.
2. Navigate to the project directory.
3. If required update the `appsettings.json` with your Hacker News API URL configuration.
   It should work using the default settings without update the file. 
4. Open a terminal or command prompt.
5. Run the following command to start the application:
   ```bash
   dotnet run
   
   Alternativly you can run it from visual studio 2022 using IIS express. 

1. The application will start on the configured port. 
   It should open up the browser at 
   http://localhost:10369/swagger/index.html   
   ( change the port if required) 

2. Click on the Get method and then click on the button (Try It Out) to allow editing the numberOfTopStories. 
   Key in an integer number into the numberOfTopStories and click on Execute. 
   You should see the top number of best stories. 


AppSettings Example : 
 "HackerNewsApiSettings": {
   "BestStoriesUrl": "https://hacker-news.firebaseio.com/v0/beststories.json",
   "StoryDetailsUrl": "https://hacker-news.firebaseio.com/v0/item/",
   "BestStoryIdsCacheExpiration": 600,
   "StoryDetailsCacheExpiration": 600,
   "ForceReSortBestStoryIds": false
 },
    
 BestStoriesUrl:  The Url to get the best story ids
     
 StoryDetailsUrl: The URL of the story details 
 BestStoryIdsCacheExpiration: BestStoryIds Cache Expiration policy in seconds 
 StoryDetailsCacheExpiration StoryDetails Cache Expiration policy in seconds 
 ForceReSortBestStoryIds: If set to true ,the Stories Controller will not assume that the stories returned from the url call to Hacker news are sorted 
      (Looking at the reponse of the Hacker news BestStoriesUrl the returned json appears to be  sorted already by the highest score so there may be no need to re-sort , 
     hence this flag allows to avoid double sorting)



Endpoints

Get Best Stories

Endpoint: /api/beststories/{n}
Method: GET
Description: Retrieves the details of the top n stories sorted by score.
Parameters:
n: Number of top stories to retrieve.

Example: 

Get request URL: http://localhost:10369/api/BestStories/2

curl -X 'GET' \
  'http://localhost:10369/api/BestStories/2' \
  -H 'accept: text/plain'

Example of a Response when n=2 :
[
  {
    "by": "todsacerdoti",
    "descendants": 470,
    "id": 38923741,
    "kids": [
      38924826,
      38924104,
      38929516,
      38924377,
      38924896,
      38924596,
      38929776,
      38927291,
      38926317,
      38925231,
      38924218,
      38928391,
      38924917,
      38925237,
      38928179,
      38924261,
      38929738,
      38924904,
      38924286,
      38924247,
      38929719,
      38927946,
      38928190,
      38926177,
      38927183,
      38927428,
      38927584,
      38926198,
      38926964,
      38934212,
      38928007,
      38932345,
      38924172,
      38924283,
      38924194,
      38925642,
      38924173,
      38928239,
      38924181,
      38928502,
      38927364,
      38924941,
      38930813
    ],
    "score": 968,
    "time": 1704789358,
    "title": "Python 3.13 Gets a JIT",
    "type": 0,
    "url": "https://tonybaloney.github.io/posts/python-gets-a-jit.html"
  },
  {
    "by": "tosh",
    "descendants": 362,
    "id": 38920043,
    "kids": [
      38920282,
      38920383,
      38920484,
      38920268,
      38920285,
      38924407,
      38922833,
      38920360,
      38921826,
      38920264,
      38921102,
      38920530,
      38923009,
      38920733,
      38921523,
      38923385,
      38920645,
      38921810,
      38920444,
      38922915,
      38923644,
      38920182,
      38924802,
      38922942,
      38920440,
      38920689,
      38921348,
      38921502,
      38923697,
      38923294,
      38920353,
      38920227,
      38920935,
      38926256,
      38925282,
      38923400,
      38925469,
      38921404,
      38922322,
      38922975,
      38921576,
      38920820,
      38926635,
      38923504,
      38922968,
      38925018,
      38920562,
      38920785,
      38921533,
      38920761,
      38921043,
      38920498,
      38933945,
      38922908,
      38920181
    ],
    "score": 881,
    "time": 1704756774,
    "title": "Polars",
    "type": 0,
    "url": "https://pola.rs/"
  }
]

Response headers:
content-type: application/json; charset=utf-8 
 date: Wed,10 Jan 2024 14:47:21 GMT 
 server: Microsoft-IIS/10.0 
 transfer-encoding: chunked 
 x-powered-by: ASP.NET 

Testing the Application
Running Unit Tests
open the dotnet CLI (visual studio 2022 developer command prompt)
Ensure you're in the project root directory.
Run the following command to execute tests:
	```bash
	dotnet test

Contributors
* Avi Ben-Margi

Improvments required if I had more time:
- Use DTO objects(Data Transformation object) for story to seperate the representation of the returned json object from Hacker news from the local implementation and the consumer
  Use Mapperly (faster than Automapper or Mapster and uses less memory) to map from StoryDto to Story domain object. 
- add more unit tests or integration tests to increase code coverage. 
  simulate exception thrown from the HackerNews URLs and return readable human errors. 
- Return mixed unsorted best stories ids and check that the force re-sort flag works as expected.
- load testing, performance testing. 
- Improve exception handling (using middleware)
- add policies to retry in case of failure to get response from hackernews urls and abort with a clear message to the user.   
- add circuit breakers to handle abusive clients using middleware. 
- load balancing 

License
This project is licensed under the MIT License.
