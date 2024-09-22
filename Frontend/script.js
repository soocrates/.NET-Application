const apiUrl = "http://localhost:5117/api"; // Change to your actual API URL

// Function to fetch categories and populate the category list and select dropdowns
async function fetchCategories() {
    const response = await fetch(`${apiUrl}/category`);
    const categories = await response.json();
    
    const categoryList = document.getElementById("categoryList");
    const categorySelect = document.getElementById("categorySelect");
    const updateCategorySelect = document.getElementById("updateCategorySelect");

    categoryList.innerHTML = '';
    categorySelect.innerHTML = '';
    updateCategorySelect.innerHTML = '';

    categories.forEach(category => {
        // Populate categories in the list
        const listItem = document.createElement("li");
        listItem.innerHTML = `${category.name} - ${category.description}
            <button onclick="deleteCategory(${category.categoryId})">Delete</button>`;
        categoryList.appendChild(listItem);

        // Populate categories in the select dropdowns
        const option = document.createElement("option");
        option.value = category.categoryId;
        option.textContent = category.name;
        categorySelect.appendChild(option);
        updateCategorySelect.appendChild(option.cloneNode(true));
    });
}

// Function to fetch products and display them
async function fetchProducts() {
    const response = await fetch(`${apiUrl}/product`);
    const products = await response.json();
    
    const productList = document.getElementById("productList");
    productList.innerHTML = '';

    products.forEach(product => {
        const listItem = document.createElement("li");
        listItem.innerHTML = `
            ${product.name} - $${product.price} - ${product.description}
            <button onclick="editProduct(${product.productId})">Edit</button>
            <button onclick="deleteProduct(${product.productId})">Delete</button>
        `;
        productList.appendChild(listItem);
    });
}

// Add a new product
document.getElementById("productForm").addEventListener("submit", async function(event) {
    event.preventDefault();

    const product = {
        name: document.getElementById("productName").value,
        price: parseFloat(document.getElementById("productPrice").value),
        imageUrl: document.getElementById("productImage").value,
        description: document.getElementById("productDescription").value,
        categoryId: parseInt(document.getElementById("categorySelect").value)
    };

    const response = await fetch(`${apiUrl}/product`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(product)
    });

    if (response.ok) {
        alert("Product added successfully!");
        fetchProducts();
    } else {
        alert("Failed to add product.");
    }
});

// Add a new category
document.getElementById("categoryForm").addEventListener("submit", async function(event) {
    event.preventDefault();

    const category = {
        name: document.getElementById("categoryName").value,
        description: document.getElementById("categoryDescription").value
    };

    const response = await fetch(`${apiUrl}/category`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(category)
    });

    if (response.ok) {
        alert("Category added successfully!");
        fetchCategories();
    } else {
        alert("Failed to add category.");
    }
});

// Delete a product
async function deleteProduct(productId) {
    const response = await fetch(`${apiUrl}/product/${productId}`, {
        method: "DELETE"
    });

    if (response.ok) {
        alert("Product deleted successfully!");
        fetchProducts();
    } else {
        alert("Failed to delete product.");
    }
}

// Delete a category
async function deleteCategory(categoryId) {
    const response = await fetch(`${apiUrl}/category/${categoryId}`, {
        method: "DELETE"
    });

    if (response.ok) {
        alert("Category deleted successfully!");
        fetchCategories();
    } else {
        alert("Failed to delete category.");
    }
}

// Edit a product (fetch data to fill the form)
async function editProduct(productId) {
    const response = await fetch(`${apiUrl}/product/${productId}`);
    const product = await response.json();

    // Populate the update form
    document.getElementById("updateProductId").value = product.productId;
    document.getElementById("updateProductName").value = product.name;
    document.getElementById("updateProductPrice").value = product.price;
    document.getElementById("updateProductImage").value = product.imageUrl;
    document.getElementById("updateProductDescription").value = product.description;
    document.getElementById("updateCategorySelect").value = product.categoryId;

    document.getElementById("updateProductSection").style.display = "block";
}

// Update a product
document.getElementById("updateProductForm").addEventListener("submit", async function(event) {
    event.preventDefault();

    const productId = document.getElementById("updateProductId").value;
    const product = {
        productId: parseInt(productId),
        name: document.getElementById("updateProductName").value,
        price: parseFloat(document.getElementById("updateProductPrice").value),
        imageUrl: document.getElementById("updateProductImage").value,
        description: document.getElementById("updateProductDescription").value,
        categoryId: parseInt(document.getElementById("updateCategorySelect").value)
    };

    const response = await fetch(`${apiUrl}/product/${productId}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(product)
    });

    if (response.ok) {
        alert("Product updated successfully!");
        document.getElementById("updateProductSection").style.display = "none";
        fetchProducts();
    } else {
        alert("Failed to update product.");
    }
});

// Initialize the page by fetching categories and products
fetchCategories();
fetchProducts();
