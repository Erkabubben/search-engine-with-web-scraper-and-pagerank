/**
 * Module for the Controller.
 *
 * @author Erik Lindholm <elimk06@student.lnu.se>
 * @author Mats Loock
 * @version 1.0.0
 */

import fetch from 'node-fetch'

/**
 * Encapsulates a controller.
 */
export class Controller {
  /**
   * Displays the index page.
   *
   * @param {object} req - Express request object.
   * @param {object} res - Express response object.
   * @param {Function} next - Express next middleware function.
   */
  async index (req, res, next) {
    try {
      res.render('main/index')
    } catch (error) {
      next(error)
    }
  }

  /**
   * Makes a Search request to the Search Engine API and returns the result JSON.
   *
   * @param {object} req - Express request object.
   * @param {object} res - Express response object.
   * @param {Function} next - Express next middleware function.
   */
  async Search (req, res, next) {
    console.log(process.env.API_URL + "/Search")
    const response = await fetch(process.env.API_URL + "/Search", {
      method: 'post',
      body: await JSON.stringify(req.body),
      headers: { 'Content-Type': 'application/json' }
    })
    res.setHeader('Content-Type', 'application/json')
    res.writeHead(200)
    res.end(await response.text())
  }
}
