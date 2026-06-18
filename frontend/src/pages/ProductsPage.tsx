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
    const [expandedProduct, setExpandedProduct] = useState<string | null>(null);
    const [quantities, setQuantities] = useState<Record<string, number>>({});

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

    const addProductToCart = async (productId: string, quantity: number) => {
        try {
            const token = localStorage.getItem("token");

            const response = await fetch(
                "https://localhost:7144/api/orders/cart",
                {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        productId,
                        quantity
                    })
                });
            console.log(JSON.stringify({
                productId,
                quantity
            }))

            if (!response.ok) {
                const error = await response.json();
                alert(error.error);
                return;
            }
            alert("Product added to cart");
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
                            className="col-md-4 mb-4">
                            <div
                                className="card shadow-sm"
                                style={{ cursor: "pointer" }}
                                onClick={() => setExpandedProduct(
                                    expandedProduct === product.productId ? null : product.productId
                                )}>

                                <div className="card-body">
                                    <h5 className="card-title">{product.name}</h5>
                                </div>

                                <div className="card-footer">
                                    <strong>{product.price}</strong>
                                </div>

                                {expandedProduct === product.productId && (
                                    <div className="card-body border-top">
                                        <p className="card-text">{product.description}</p>

                                        <div className="d-flex align-items-center gap-2">
                                            
                                            <input
                                                type="number"
                                                className="form-control"
                                                style={{ width: "80px" }}
                                                value={quantities[product.productId] ?? 1}
                                                onClick={(e) => e.stopPropagation()}
                                                onChange={(e) => {
                                                    setQuantities({
                                                        ...quantities, [product.productId]: Number(e.target.value)
                                                    })
                                                }}></input>

                                            <button
                                                className="btn btn-primary mt-3 w-100"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    addProductToCart(product.productId, quantities[product.productId] ?? 1)
                                                }}
                                            >Add to cart</button>
                                        </div>
                                    </div>
                                )}
                            </div>

                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}

export default ProductsPage;