# HackerNewsAPI

I was given a task as part of an interview process for a c# developer at a reputable bank.
### Task:
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
	- Visual studio 2022 with SDK 8.0.101  OR 
 	- dotnet CLI version 8.0.101 

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
	
### AppSettings: 
The app settings are in appsettings.json
	  
	  "Logging": {
	    "LogLevel": {
	      "Default": "Information",
	      "Microsoft": "Warning",
	      "Microsoft.Hosting.Lifetime": "Information"
	    }
	
	  },
	  "HackerNewsStoryProviderSettings": {
	    "BestStoriesUrl": "https://hacker-news.firebaseio.com/v0/beststories.json",
	    "StoryDetailsUrl": "https://hacker-news.firebaseio.com/v0/item/",
	    "BestStoryIdsCacheExpiration": 600,
	    "StoryDetailsCacheExpiration": 600
	  },
	
	  "HackerNewsApiSettings": {
	    "ForceReSortBestStoryIds": false
	  },
	  "AllowedHosts": "*"
	
	

 HackerNewsStoryProviderSettings: 
 
	 BestStoriesUrl:  The Url to get the best story ids     
	 StoryDetailsUrl: The URL of the story details 
	 BestStoryIdsCacheExpiration: BestStoryIds Cache Expiration policy in seconds 
	 StoryDetailsCacheExpiration StoryDetails Cache Expiration policy in seconds 
	 
HackerNewsApiSettings : 

	ForceReSortBestStoryIds: If set to true ,the Stories Controller will not assume that the stories returned from the url call to Hacker news are sorted 
	      (Looking at the reponse of the Hacker news BestStoriesUrl the returned json appears to be  sorted already by the highest score so there may be no need to re-sort , 
	     hence this flag allows to avoid double sorting)



### Endpoints

HTTP Get Best Stories

Endpoint: /api/beststories/{n}
Method: GET
Description: Retrieves the details of the top n stories sorted by score.
Parameters:
n: Number of top stories to retrieve.

Example: 

Get request URL: http://localhost:10369/api/BestStories/3

curl -X 'GET' \
  'http://localhost:10369/api/BestStories/3' \
  -H 'accept: text/plain'

#### Example of a Response when n=3 :
	[
	  {
	    "title": "Python 3.13 Gets a JIT",
	    "uri": "https://tonybaloney.github.io/posts/python-gets-a-jit.html",
	    "postedBy": "todsacerdoti",
	    "time": 1704789358,
	    "score": 1016,
	    "commentCount": 45
	  },
	  {
	    "title": "I pwned half of America's fast food chains simultaneously",
	    "uri": "https://mrbruh.com/chattr/",
	    "postedBy": "MrBruh",
	    "time": 1704843698,
	    "score": 940,
	    "commentCount": 48
	  },
	  {
	    "title": "Polars",
	    "uri": "https://pola.rs/",
	    "postedBy": "tosh",
	    "time": 1704756774,
	    "score": 912,
	    "commentCount": 57
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

### Contributors
* Avi Ben-Margi

### Improvments required - if I had more time:
- Consider to cache the HackerNews story detail DTO or the HackerNews Story details Model and not the story details respone DTO. Storing the response DTO is faster but if new
 using the response DTO allows better performance and thats why I used it but using the hacker news DTO or the model will allow better maintainability should the hacker news api reponses change or different requirements are raised by the user to return different responses which rely on the original data , we won't have the original HackerAPI response object.
 The benefit of using the response object is speed because there are less heap object allocations and garbage collection will be faster as we create less objects. 
-  Use object mapping frameworks and not manual DTO mapping - for example Mapperly (faster than Automapper or Mapster and uses less memory) to map from StoryDto to Story domain object. 
I used manual mapping using a static util function. In such a small app its ok and provides the best performance.
- add more unit tests or integration tests to increase code coverage. 
  simulate exception thrown from the HackerNews URLs and return readable human errors. 
- load testing, performance testing. 
- Improve exception handling (using middleware)
- add policies to retry in case of failure to get response from hackernews urls and abort with a clear message to the user.   
- add circuit breakers to handle abusive clients using middleware. 
- load balancing 
- add Unit test Simulate a scenario in which the hacker news API doesn't return sorted best stories and check that the force re-sort flag works as expected.
- improve logging. I showed example of use in a few places but its not enough. 

# License
	This project is licensed under the MIT License.
