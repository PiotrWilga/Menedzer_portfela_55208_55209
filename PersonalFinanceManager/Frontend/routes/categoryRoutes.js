const express = require('express');
const router = express.Router();
const categoryController = require('../controllers/categoryController');
const { ensureAuthenticated } = require('../controllers/authController'); 

router.use(ensureAuthenticated);

router.get('/', categoryController.renderCategoriesPage);

router.get('/add', categoryController.renderAddCategoryPage);
router.post('/', categoryController.addCategory);

router.get('/:id/edit', categoryController.renderEditCategoryPage);
router.post('/:id', categoryController.updateCategory); 

router.delete('/:id', categoryController.deleteCategory); 

module.exports = router;