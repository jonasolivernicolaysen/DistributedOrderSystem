import { useEffect, useState } from "react";

function ProductsPage() {
    // state
    interface Product {
        productId: string,
        name: string,
        description: string,
        price: number
    }

    const [products, setProducts] = useState([]);

    // functions
    const showProducts = async () => {
        try {
            const token = localStorage.getItem("token");
;            
            const response = await fetch(
                "https://localhost:7144/api/products",
                {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    }
                });

            if (!response.ok) {
                alert(`Request failed: ${response.status}`);
                return;
            }
            const data = await response.json();

            setProducts(data);

        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        }
    };

    useEffect(() => {
        showProducts();
    }, []);

    return (
        <div>
            <h1>Products</h1>

            <div className="container mt-4">
                <div className="row">
                    {products.map((product: Product) => (
                        <div
                            key={product.productId}
                            className="col-md-4 mb-4"
                        >
                            <div
                                className="card h-100 shadow-sm"
                                style={{ cursor: "pointer" }}
                                onClick={() => console.log(product.productId)}
                            >
                                <div className="card-body">
                                    <h5 className="card-title">
                                        {product.name}
                                    </h5>

                                    <p className="card-text">
                                        {product.description}
                                    </p>
                                </div>

                                <div className="card-footer">
                                    <strong>{product.price} kr</strong>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}

export default ProductsPage;