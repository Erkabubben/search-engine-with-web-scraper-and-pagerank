# Project

In the project, you decide what you want to work on! There are several pre-defined projects you can select from. Choose one of them!

<!----------------------------------------------------------------------------->
<details>
  <summary><h2>P1 - Recommendation System</h2></summary>

This is one of the pre-defined project ideas you can choose for your project.

### Recommendation system for MovieLens

Modify your recommendation system from Assignment 1 to use the small MovieLens dataset with 100 000 ratings. You can read about the dataset [here](https://grouplens.org/datasets/movielens/).

You are only required to use user-based collaborative filtering and not item-based (since pre-calculating matching movies will take a very long time).

The dataset can be downloaded on the [Datasets](https://coursepress.lnu.se/courses/web-intelligence/assignments/datasets) page.

<Hint type="warning"><b>Note!</b> A problem with the calculations used in Assignment 1 is that if many users have rated many movies, as in the MovieLens dataset, many movies will get the max recommendation score of 5. To make better recommendations, you can do some modifications:
<ul>
<li>Only include users with the similarity of more than 0 in the calculations.</li>
<li>Exclude all movies with very few ratings.</li>
</ul>
</Hint>

### Grading

<table>
<tr>
    <th>Grade</th>
    <th>Requirements</th>
</tr>
<tr>
    <td>E</td>
    <td>
    <ul>
        <li>Use the same Recommendation System you developed for Assignment 1 with the MovieLens dataset.</li>
        <li>Add code for storing the number of ratings each movie has.</li>
        <li>Modify the score calculation to exclude movies with few ratings.</li>
        <li>It shall be possible to set the min number of ratings in the client GUI.</li>
    </ul>
    </td>
</tr>
<tr>
    <td>C-D</td>
    <td>
    <ul>
    <li>If you set min number of ratings to 1 you will get lots of results with max rating of 5.</li>
    <li>To improve the results you shall:
        <ol>
        <li>round the score to four decimals</li>
        <li>sort the result list by score (highest first)</li>
        <li>if two results have equal score, sort by number of ratings (highest first).</li>
        </ol>
    </li>
    <li>You must show number of ratings for each movie in the result list in your client GUI.</li>
    </ul>
    </td>
</tr>
<tr>
    <td>A-B</td>
    <td>
    <ul>
        <li>Measure the time it takes to find top five recommended movies for a user (try for example user 256).</li>
        <li>Build a cache for similarity calculations to avoid calculating similarity between two users more than once.</li>
        <li>How much does the cache improve execution times when finding top five recommended movies?</li>
    </ul>
    </td>
</tr>
</table>
</details>

<!----------------------------------------------------------------------------->
<details>
  <summary><h2>P2 - Clustering</h2></summary>

This is one of the pre-defined project ideas you can choose for your project.

### Clustering Wikipedia articles

Modify your clustering system from Assignment 2 to use Wikipedia articles (90 articles about Programming, 90 about Games). The dataset can be downloaded on the [Datasets](https://coursepress.lnu.se/courses/web-intelligence/assignments/datasets) page.

To use the dataset for clustering, you need to select some words and calculate the frequency of these words in each Wikipedia article. It is not recommended to use all words from the articles since similarity calculations will then take a long time. You can, for example, use the following words:

`language, programming, computer, software, hardware, data, player, online, system, development, machine, console, developer, design, history, technology, standard, information, article, example`

The article *Arcade_game* would then have the following frequencies:

`0;4;14;1;58;1;11;7;12;4;9;17;0;5;33;8;1;2;7;1`

### Grading

<table>
  <tr>
    <th>Grade</th>
    <th>Requirements</th>
  </tr>
  <tr>
    <td>E</td>
    <td>
      <ul>
        <li>Read all articles about programming and games and convert each article to word frequencies using the word list above.</li>
        <li>Perform k-means clustering on the 180 articles using two clusters.</li>
        <li>Are the articles well separated into one cluster of gaming related articles and one cluster about programming?</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>C-D</td>
    <td>
      <ul>
        <li>Perform hierarchical clustering on the 180 articles.</li>
        <li>Are articles about similar topics well separated into branches?</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>A-B</td>
    <td>
      <ul>
        <li>Generate your own word list of at least 100 words.</li>
        <li>Repeat k-means and hierarchical clustering using the new word list.</li>
        <li>Are the results better with the new word list?</li>
      </ul>
    </td>
  </tr>
</table>
</details>

<!----------------------------------------------------------------------------->
<details>
  <summary><h2>P3 - Text Classification</h2></summary>

This is one of the pre-defined project ideas you can choose for your project.

### Text classification of Wikipedia articles

You are required to use Python and Scikit-learn for this project.

Classify the Wikipedia 300 dataset (150 articles about Video games, 150 about Programming) using machine learning. The dataset can be downloaded at the [Datasets](https://coursepress.lnu.se/courses/web-intelligence/assignments/datasets) page.

For text classification, the bag-of-words approach where you convert an article to word counts is typically used. An improvement is TF-IDF (Term Frequency-Inverse Document Frequency), which converts from word counts to word frequencies. TF-IDF is especially useful if the size of the articles varies a lot. Suitable algorithms for text classification are Multinomial Naïve Bayes (MultinomialNB) and Support Vector Machines with linear kernels (LinearSVC).

You can read about text classification in Scikit-learn [here](https://scikit-learn.org/stable/tutorial/text_analytics/working_with_text_data.html).

### Grading

<table>
  <tr>
    <th>Grade</th>
    <th>Requirements</th>
  </tr>
  <tr>
    <td>E</td>
    <td>
      <ul>
        <li>Classify the dataset using MultinomailNB and LinearSVC with the bag-of-words approach</li>
        <li>Evaluate accuracy on the same data as used for training the algorithms</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>C-D</td>
    <td>
      <ul>
        <li>Also evaluate accuracy using 10-fold cross validation.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>A-B</td>
    <td>
      <ul>
        <li>Use TF-IDF to convert from word counts to word frequencies.</li>
        <li>Does TF-IDF improve classification accuracy when using cross-validation?</li>
      </ul>
    </td>
  </tr>
</table>
</details>

<!----------------------------------------------------------------------------->
<details>
  <summary><h2>P4 - Web scraping</h2></summary>

This is one of the pre-defined project ideas you can choose for your project.

### Web scraping

In this project, you shall use a web scraping library to download articles that can be used in your search engine from Assignment 3.

If you use Python, the [BeautifulSoup](https://www.crummy.com/software/BeautifulSoup/bs4/doc/) library is very powerful and easy to use. A quick start guide can be found [here](https://realpython.com/python-web-scraping-practical-introduction/). For Java can check out [HtmlUnit](http://htmlunit.sourceforge.net/). A quick start guide can be found [here](https://ksah.in/introduction-to-web-scraping-with-java/).

When scraping a site such as Wikipedia, you usually start on one page and follow all outgoing links.

You can download pages from Wikipedia or any other site.

### Grading

<table>
  <tr>
    <th>Grade</th>
    <th>Requirements</th>
  </tr>
  <tr>
    <td>E</td>
    <td>
      <ul>
        <li>Scrape and store raw HTML for at least 200 pages.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>C-D</td>
    <td>
      <ul>
        <li>Parse the raw HTML files to generate a dataset similar to the Wikipedia dataset from Assignment 3.</li>
        <li>For each article, the dataset shall contain a file with all words in the article and another file with all outgoing links in the article.</li>
      </ul>
    </td>
  </tr>
  <tr>
    <td>A-B</td>
    <td>
      <ul>
        <li>Use the dataset with your search engine from Assignment 3.</li>
        <li>Use both content-based ranking and PageRank to rank search results.</li>
      </ul>
    </td>
  </tr>
</table>
</details>
