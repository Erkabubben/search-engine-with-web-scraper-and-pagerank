/**
 * OnClick function for the Search button.
 */
async function button0() {
    setSpinnerAndResultsTableVisibility(false)
    const responseJSON = await fetchPostRequest("./Search")
    removeChildren(resultsTable)
    addRowToTable(resultsTable, ['Link', 'Score', 'Content', 'Location'], true)
    for (let i = 0; i < responseJSON.pages.length; i++) {
        const page = responseJSON.pages[i]
        addRowToTable(resultsTable, [
            page.pageName,
            page.finalScore.toFixed(2),
            page.contentScore.toFixed(2),
            page.locationScore.toFixed(2)],
            false)
    }
    resultsTable.removeAttribute('hidden')
    setSpinnerAndResultsTableVisibility(true)
}

/**
 * Adds a new row to the table based on the provided text content.
 *
 * @param {Element} table - The table to add the new row to.
 * @param {Array} textContent - Array of strings that will be used as row elements.
 * @param {bool} isHeader - Whether the new row should be a header row or a regular row.
 */
function addRowToTable (table, textContent, isHeader) {
    const row = document.createElement('tr')
    for (let i = 0; i < textContent.length; i++) {
        const element = textContent[i];
        const newElement = isHeader ? document.createElement('th') : document.createElement('td')
        if (!isHeader && i == 0) {
            const anchor = document.createElement('a')
            anchor.textContent = element
            anchor.setAttribute('href', 'https://en.wikipedia.org/wiki/' + element)
            newElement.appendChild(anchor)
        } else {
            newElement.textContent = element
        }
        row.appendChild(newElement)
    }
    table.appendChild(row)
}

/**
 * Sets the visibility of the results table and the loading spinner.
 *
 * @param {bool} setTableToVisible - Whether to set the table to visible and spinner to hidden or vice versa.
 */
function setSpinnerAndResultsTableVisibility(setTableToVisible) {
    if (setTableToVisible) {
        spinnerContainer.setAttribute('hidden', 'true')
        resultsTable.removeAttribute('hidden')
    } else {
        spinnerContainer.removeAttribute('hidden')
        resultsTable.setAttribute('hidden', 'true')
    }
}

/**
 * Makes a POST request to the provided URL and returns the response as JSON.
 * 
 * @param {string} url - The url to make the request to.
 */
 async function fetchPostRequest (url) {
    const formDataString = await JSON.stringify(getFormData())
    const response = await fetch(url, {
        method: 'post',
        body: formDataString,
        headers: { 'Content-Type': 'application/json' }
    })
    return await response.json()
}

/**
 * Removes all children of a DOM node.
 * 
 * @param {Element} node - The node to remove all children from.
 */
function removeChildren(node) {
    while (node.lastChild !== null) {
        node.removeChild(node.lastChild)
    }
}

/**
 * Retrieves data from the form element and returns it as a JavaScript object.
 */
function getFormData() {
    return {
        query: document.querySelector('#query-input').value,
        maxAmount: document.querySelector('#results').value,
    }
}

var resultsTable = document.querySelector('#results-table')
document.querySelector('#button-search').addEventListener('click', button0)

var spinnerContainer = document.querySelector('#spinner-container')
spinnerContainer.setAttribute('hidden', 'true')
